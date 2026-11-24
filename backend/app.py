from flask import Flask, jsonify, request
from services.tts_engine import generate_audio
from db_config import DB_CONFIG
import mysql.connector
import random
import pyttsx3
from datetime import datetime

app = Flask(__name__)

# ================= DB =================
def get_connection():
    return mysql.connector.connect(**DB_CONFIG)

# ================= HOME =================
@app.route('/')
def home():
    return jsonify({"message": "Backend Running!"})

# ================= MCQ =================
@app.route('/get_mcq/<domain>', methods=['GET'])
def get_mcq(domain):
    try:
        conn = get_connection()
        cursor = conn.cursor(dictionary=True)

        query = "SELECT * FROM question WHERE option_a IS NOT NULL"
        cursor.execute(query)
        questions = cursor.fetchall()

        if not questions:
            return jsonify([{
                "question": "Fallback question?",
                "options": [
                    {"text": "Option A", "score": 4},
                    {"text": "Option B", "score": 2},
                    {"text": "Option C", "score": 0},
                    {"text": "Option D", "score": 0}
                ]
            }])

        random.shuffle(questions)
        questions = questions[:10]

        final = []
        for q in questions:
            options = [
                {"text": q['option_a'], "score": 4},
                {"text": q['option_b'], "score": 2},
                {"text": q['option_c'], "score": 0},
                {"text": q['option_d'], "score": 0},
            ]
            random.shuffle(options)

            final.append({
                "question": q['question_text'],
                "options": options
            })

        return jsonify(final)

    except Exception as e:
        print("MCQ ERROR:", e)
        return jsonify({"error": str(e)}), 500

    finally:
        cursor.close()
        conn.close()

# ================= SPEAKING =================
@app.route('/get_speaking', methods=['GET'])
def get_speaking():
    try:
        domain_ids_str = request.args.get('domain_ids', None)
        difficulty = request.args.get('difficulty', None)

        conn = get_connection()
        cursor = conn.cursor(dictionary=True)

        query = "SELECT question_text FROM question WHERE option_a IS NULL"
        params = []

        if domain_ids_str:
            domain_ids = [int(d) for d in domain_ids_str.split(',')]
            placeholders = ','.join(['%s'] * len(domain_ids))
            query += f" AND domain_id IN ({placeholders})"
            params.extend(domain_ids)

        if difficulty:
            query += " AND difficulty_level = %s"
            params.append(int(difficulty))

        cursor.execute(query, params)
        questions = cursor.fetchall()

        random.shuffle(questions)
        selected = questions[:10]

        final = [{"question": q["question_text"], "options": []} for q in selected]
        return jsonify(final)

    except Exception as e:
        print("SPEAKING ERROR:", e)
        return jsonify({"error": str(e)}), 500

    finally:
        cursor.close()
        conn.close()

@app.route('/speak', methods=['POST'])
def speak():
    try:
        #text = request.form.get("text")
        text = request.form.get("text")
        if not text or text.strip() == "":
            print("ERROR: Empty text received")
            return jsonify({"error": "Empty text"}), 400
        avatar_id = int(request.form.get("avatar_id", 0))

        filename = generate_audio(text, avatar_id)

        return jsonify({
            "status": "success",
            "audio": filename
        })

    except Exception as e:
        print("TTS ERROR:", e)
        return jsonify({"error": str(e)}), 500


# ================= SESSION SAVE =================
@app.route('/save_session', methods=['POST'])
def save_session():
    try:
        data = request.get_json()

        if not data:
            return jsonify({"error": "No data received"}), 400

        print("Session Data:", data)

        username = data.get("username", "Unknown")
        duration = int(data.get("duration", 0))
        times = data.get("times", [])

        conn = get_connection()
        cursor = conn.cursor()

        # Insert session
        cursor.execute(
            "INSERT INTO sessions (username, duration, created_at) VALUES (%s, %s, %s)",
            (username, duration, datetime.now())
        )
        session_id = cursor.lastrowid

        # Insert per-question times
        for idx, t in enumerate(times, start=1):
            cursor.execute(
                "INSERT INTO session_question_times (session_id, question_number, duration) VALUES (%s, %s, %s)",
                (session_id, idx, float(t))   # 👈 FIX
            )

        conn.commit()

        return jsonify({
            "status": "saved",
            "session_id": session_id
        })

    except Exception as e:
        print("SESSION ERROR:", e)
        return jsonify({"error": str(e)}), 500

    finally:
        cursor.close()
        conn.close()

# ================= SCORE =================

@app.route('/submit_score', methods=['POST'])
def submit_score():
    try:
        data = request.get_json()

        print("\n========== MCQ DATA RECEIVED FROM UNITY ==========")
        print(data)
        print("================================================\n")

        answers = data.get("answers", [])

        total = sum(a.get("score", 0) for a in answers)
        percentage = (total / (len(answers) * 4)) * 100 if answers else 0

        return jsonify({
            "total_score": total,
            "percentage": percentage
        })

    except Exception as e:
        print("SCORE ERROR:", e)
        return jsonify({"error": str(e)}), 500



# ================= RUN =================
if __name__ == '__main__':
    app.run(host="0.0.0.0", port=5000, debug=True)