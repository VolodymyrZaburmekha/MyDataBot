from fastapi import FastAPI
from llama_index import GPTVectorStoreIndex, SimpleDirectoryReader, StorageContext, load_index_from_storage
import os
from openai.error import RateLimitError
from pydantic import BaseModel
from config import get_lama_indexes_folder
from enum import StrEnum
from tenacity import RetryError


class AiResultType(StrEnum):
    OK = "Ok"
    RATE_LIMIT_ERROR = "RateLimitError"
    UNKNOWN_ERROR = "UnknownError"


def get_answer(
        # open_ai_key,
        index_folder_name,
        query):
    index_folder_path = os.path.join(
        get_lama_indexes_folder(), index_folder_name)

    # rebuild storage context
    storage_context = StorageContext.from_defaults(
        persist_dir=index_folder_path)
    # load index
    index = load_index_from_storage(storage_context)

    query_engine = index.as_query_engine()
    # use query variable to set the query
    try:
        response = query_engine.query(query).response
        return (response, AiResultType.OK)
    except RetryError as retryError:  # RateLimitError
        return ("", AiResultType.RATE_LIMIT_ERROR) if isinstance(retryError.__cause__, RateLimitError) else (
        "", AiResultType.UNKNOWN_ERROR)
