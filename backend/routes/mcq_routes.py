from flask import Blueprint, request, jsonify
from services.question_service import get_question_by_difficulty
from services.tts_engine import generate_audio

mcq_bp = Blueprint('mcq', __name__)

# START MCQ
@mcq_bp.route('/start-mcq', methods=["GET"])
def start_mcq():

    difficulty = 1  # start from easy

    question = get_question_by_difficulty(difficulty)

    #generate_audio(question["question_text"], 0)

    return jsonify({
        "question": question,
        "difficulty": difficulty
    })


# NEXT QUESTION (ADAPTIVE)
@mcq_bp.route('/next-mcq', methods=['POST'])
def next_mcq():

    data = request.json

    selected_option = data.get("selected_option")
    current_difficulty = data.get("difficulty")

    # Adaptive Logic
    if selected_option in ["A", "B"]:
        new_difficulty = min(current_difficulty + 1, 3)

    elif selected_option == "C":
        new_difficulty = current_difficulty

    else:  # D
        new_difficulty = max(current_difficulty - 1, 1)

    question = get_question_by_difficulty(new_difficulty)

    generate_audio(question["question_text"], 0)

    return jsonify({
        "question": question,
        "difficulty": new_difficulty
    })