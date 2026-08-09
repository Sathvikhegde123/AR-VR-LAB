# VantageVR – Virtual Reality Interview Simulator

VantageVR is a Virtual Reality-based interview simulator developed using Unity and a hybrid backend system. It simulates real interview scenarios by delivering domain-specific questions, adaptive MCQ evaluation, and audio interaction using Text-to-Speech (TTS).

The system integrates PHP for authentication and Python Flask for interview logic, connected through a shared MySQL database.

For demonstration purposes, the project is currently executed on a PC environment.

---

## 🚀 Tech Stack

**Frontend:**

- Unity (C#)

**Backend:**

- Python (Flask API)
- PHP (for Authentication, via WAMP Server)
- MySQL (via WAMP Server)
- `mysql-connector-python`

**Audio:**

- `pyttsx3` (Text-to-Speech)

---

## ✨ Project Features

### 1. Interview Mode

- User selects up to 3 domains and a difficulty level.
- The backend fetches 10 random questions based on the selection.
- Questions are delivered to the user via Text-to-Speech (TTS).
- A session report is generated at the end, including the username and duration.

### 2. MCQ Mode (Adaptive)

- Questions are fetched one at a time.
- The difficulty of the next question adapts based on the user's answer to the current one:
  - **Best / Good:** Increase difficulty
  - **Average:** Same difficulty
  - **Wrong:** Decrease difficulty
- A weighted scoring system is used:
  - **Best:** 4 points
  - **Good:** 3 points
  - **Average:** 2 points
  - **Wrong:** 0 points
- The final output is the total score.

---

## 📂 Project Structure

```
backend/
│
├── app.py
├── db_config.py
├── requirements.txt
│
├── routes/
│   ├── interview_routes.py
│   ├── mcq_routes.py
│   ├── score_routes.py
│   └── session_routes.py
│
├── services/
│   ├── question_service.py
│   ├── scoring_service.py
│   ├── session_service.py
│   └── tts_engine.py
│
├── audio/ (optional)
└── venv/
```

**Authentication (PHP):**
The PHP authentication files are located at `C:\wamp64\www\unity_api`.

---

## 🛠️ Setup and Installation

### 1. Database Setup (WAMP Server)

1.  Start **WAMP Server** and wait for the icon to turn **GREEN**.
2.  Open your web browser and navigate to `http://localhost/phpmyadmin`.
3.  Create a new database named `csd056`.
4.  Import the provided SQL file into the `csd056` database.
5.  Ensure the following tables exist:
    - `user`
    - `domain`
    - `question`
    - `session`
    - `session_question_time`
6.  Verify the database configuration in `backend/db_config.py`:
    ```python
    host = "localhost"
    user = "root"
    password = ""
    database = "csd056"
    ```

### 2. Authentication Module (PHP)

1.  Create a folder named `unity_api` inside your WAMP `www` directory (`C:\wamp64\www`).
2.  Place the authentication PHP files into `C:\wamp64\www\unity_api`.
3.  Ensure WAMP Server is running.
4.  You can test the authentication by navigating to `http://localhost/unity_api/` and performing a signup and login.

### 3. Dependency Installation

Before running the backend server, make sure to install the required Python packages.

1. Open a terminal in the `backend` folder.
2. Run the following command to install dependencies from `requirements.txt`:
   ```bash
   pip install -r requirements.txt
   ```
3. If the above command fails, you can install the packages manually:
   ```bash
   pip install flask flask-cors pymysql pyttsx3 mysql-connector-python
   ```

---

## ▶️ How to Run the Project

Follow these steps in order:

### Step 1: Start the Database

- Launch **WAMP Server** and ensure its status icon is **GREEN**.

### Step 2: Start the Backend Server

1.  Open a terminal in the `backend` folder.
2.  Activate the virtual environment:
    ```bash
    venv\Scripts\activate
    ```
3.  Run the Flask application:
    ```bash
    python app.py
    ```
4.  Confirm that the server is running by visiting `http://127.0.0.1:5000` in your browser.

### Step 3: Run the Unity Application

1.  Navigate to the Unity Build folder.
2.  Ensure `VantageVR.exe` and the `VantageVR_Data` folder are present.
3.  Double-click `VantageVR.exe` to run the application.
4.  If the application is blocked by Windows, you may need to right-click and select **Run as Administrator** or click "Run Anyway".

**Dummy Login Credentials:**

- **Username:** `admin`
- **Password:** `Admin@123`

---

## 🔌 API Endpoints

### Interview

- `POST /interview`: Fetch questions for an interview session.
- `POST /session`: Save the interview session details.

### MCQ

- `GET /start-mcq`: Start a new MCQ round.
- `POST /next-mcq`: Get the next adaptive question.
- `POST /calculate_score`: Calculate and save the final score.

---

## Control Flow

```
User → PHP Authentication → MySQL
  ↓
Unity Application
  ↓
Flask Backend
  ↓
MySQL Database
```

---

## ✅ Testing Checklist

- [ ] WAMP server is running.
- [ ] Database has been imported correctly.
- [ ] PHP authentication is working (signup/login).
- [ ] Backend Flask server is running.
- [ ] Questions are loading properly in the application.
- [ ] MCQ adaptive logic is functioning as expected.
- [ ] Text-to-Speech audio is working.
- [ ] Score calculation is correct.
- [ ] Session report is generated successfully.

---

## 🧑‍💻 Author Contribution

**Backend Development:**

- Database design and schema (MySQL)
- Flask API development for all features
- Implementation of the adaptive MCQ logic
- Integration of the Text-to-Speech engine
- Session management and reporting
- Designed and implemented the modular backend architecture

---

## ⚠️ Important Notes

- Always start the services in this order: **Database → Backend → Application**.
- WAMP Server must be running for both the database and the PHP authentication to work.
- The backend server must be running before you launch the Unity application.
- Do not delete the `VantageVR_Data` folder, as it is required by the executable.
- Keep all build files (`.exe`, `_Data` folder) in the same directory.

---

## 📝 License

This project is licensed under the Creative Commons CC0 1.0 Universal License - see the [LICENSE](LICENSE) file for details.
