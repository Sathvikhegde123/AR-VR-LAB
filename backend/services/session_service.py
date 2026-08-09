import mysql.connector

def get_connection():
    return mysql.connector.connect(
        host="localhost",
        user="root",
        password="",   # SAME as above
        database="csd056"
    )


def create_session(username, duration):

    conn = get_connection()
    cursor = conn.cursor()

    query = """
    INSERT INTO session (username, duration)
    VALUES (%s, %s)
    """

    cursor.execute(query, (username, duration))
    conn.commit()

    session_id = cursor.lastrowid

    cursor.close()
    conn.close()

    return session_id