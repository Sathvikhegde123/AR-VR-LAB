import random

def get_mcq_questions(domain):

    conn = get_connection()
    cursor = conn.cursor(dictionary=True)

    query = """
    SELECT * FROM csd056
    WHERE domain = %s AND id >= 21
    """

    cursor.execute(query, (domain,))
    questions = cursor.fetchall()

    random.shuffle(questions)
    questions = questions[:10]

    for q in questions:
        options = [
            {
                "text": q['option1'],
                "is_correct": q['correct_option'] == 1,
                "score": 4
            },
            {
                "text": q['option2'],
                "is_correct": q['correct_option'] == 2,
                "score": 4
            },
            {
                "text": q['option3'],
                "is_correct": q['correct_option'] == 3,
                "score": 4
            },
            {
                "text": q['option4'],
                "is_correct": q['correct_option'] == 4,
                "score": 4
            }
        ]

        # 👉 OPTIONAL OK OPTION
        if 'ok_option' in q:
            for i, opt in enumerate(options):
                if q['ok_option'] == i+1:
                    opt["score"] = 2

        # wrong = 0
        for opt in options:
            if not opt["is_correct"] and opt.get("score") != 2:
                opt["score"] = 0

        random.shuffle(options)

        q['options'] = options

    cursor.close()
    conn.close()

    return questions