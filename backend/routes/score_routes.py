from flask import Blueprint, request, jsonify
from services.scoring_service import calculate_score

score_bp = Blueprint('score', __name__)

@score_bp.route('/calculate_score', methods=['POST'])
def score():

    data = request.json
    answers = data.get("answers", [])

    total, percentage = calculate_score(answers)

    return jsonify({
        "total_score": total,
        "percentage": percentage
    })