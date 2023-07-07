import os


def get_lama_indexes_folder():
    return os.environ["LAMA_INDEXES_FOLDER"]


def get_nats_connection():
    return os.getenv('NATS_SERVER_URL', 'localhost')
