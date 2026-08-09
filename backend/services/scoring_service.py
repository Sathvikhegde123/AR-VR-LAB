def calculate_score(answers):
    if not answers:
        return 0, 0

    total = 0

    for ans in answers:
        total += ans.get("selected_score", 0)

    max_score = len(answers) * 4

    percentage = round((total / max_score) * 100, 2) if max_score > 0 else 0

    return total, percentage