from flask import Blueprint, request, jsonify
from services.session_service import create_session

session_bp = Blueprint('session', __name__)

@session_bp.route('/session', methods=['GET','POST'])
def session():

    if request.method == 'GET':
        return "Session route working! Use POST for data."

    data = request.json
    username = data.get("username")
    duration = data.get("duration")

    session_id = create_session(username, duration)

    return jsonify({
        "session_id": session_id,
        "username": username,
        "duration": duration
    })
@session_bp.route('/test-session')
def test_session():
    session_id = create_session("Asthaa", 30)
    return f"Session created with ID {session_id}"