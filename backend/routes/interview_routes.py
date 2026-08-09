from flask import Blueprint, request, jsonify
from services.question_service import get_interview_questions
from services.tts_engine import generate_audio

interview_bp = Blueprint('interview', __name__)

@interview_bp.route('/interview', methods=['POST'])
def interview():

    data = request.json
    domains = data.get("domains")
    difficulty = data.get("difficulty")

    questions = get_interview_questions(domains, difficulty)

    for i, q in enumerate(questions):
        avatar_id = i % 4
        generate_audio(q["question_text"], avatar_id)

    return jsonify(questions)