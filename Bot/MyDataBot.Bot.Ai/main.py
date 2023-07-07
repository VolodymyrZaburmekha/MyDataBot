import asyncio
import nats
from nats.errors import TimeoutError
from nats.js import JetStreamContext
import json
from nats.aio.msg import Msg
from nats.aio.client import Client
from nats.js.api import StorageType
from lama import get_answer, AiResultType
import os
from config import get_nats_connection


STREAM_NAME = "MESSAGE_PROCESSING"
SUBSCRIPTION_SUBJECT = "new-message-to-process"
MESSAGE_PROCESSED_SUBJECT = "message-processed"
MESSAGE_PROCESS_QUEUE = "message-process-queue"

class ProcessedMessageDto:
    def __init__(self, correlation_id: str, message_id: str, message_text: str, message_response: str, ai_result_type: AiResultType) -> None:
        self.correlationId = correlation_id
        self.messageId = message_id
        self.messageText = message_text
        self.messageResponse = message_response
        self.resultType = ai_result_type.__str__()

    def from_message_for_processing(message_for_processing, response: str, result_type: AiResultType):
        return ProcessedMessageDto(
            message_for_processing['CorrelationId'],
            message_for_processing['MessageId'],
            message_for_processing['MessageText'],
            response,
            ai_result_type=result_type
        )

async def create_subscription(js: JetStreamContext):
    subscription = await js.subscribe(
        subject=SUBSCRIPTION_SUBJECT,
        queue=MESSAGE_PROCESS_QUEUE,
        durable=MESSAGE_PROCESS_QUEUE,
        stream=STREAM_NAME,
    )
    return subscription


async def process_message(nats_client: Client, msg: Msg):
    data_str = msg.data.decode()
    print("process_message")
    # await asyncio.sleep(1)
    bot_msg = json.loads(data_str)

    (response, result_type) = get_answer(
        index_folder_name=bot_msg["DataFolder"], query=bot_msg["MessageText"])

    processed_message = ProcessedMessageDto.from_message_for_processing(
        message_for_processing=bot_msg, response=response, result_type=result_type)

    response_str = json.dumps(processed_message.__dict__)
    print(response_str)
    await nats_client.publish(subject=MESSAGE_PROCESSED_SUBJECT, payload=bytes(response_str, 'UTF-8'))
    await msg.ack()


async def main():
    nc = await nats.connect(get_nats_connection())

    # Create JetStream context.
    js = nc.jetstream()
    await js.add_stream(
        name="MESSAGE_PROCESSING",
        subjects=[SUBSCRIPTION_SUBJECT, MESSAGE_PROCESSED_SUBJECT],
        storage=StorageType.MEMORY)

    subscription = await create_subscription(js)
    while True:
        try:
            msg = await subscription.next_msg(10)
            print("NEW_MESSAGE")
            asyncio.create_task(process_message(nats_client=nc, msg=msg))
        except TimeoutError:
            print("timeout")

if __name__ == '__main__':
    asyncio.run(main())
