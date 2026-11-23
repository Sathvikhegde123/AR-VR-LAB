import pyttsx3
import os
import time
import threading
import traceback
from dotenv import load_dotenv


tts_lock = threading.Lock()

load_dotenv()

unity_project_path = os.getenv("UNITY_PROJECT_PATH")

# Prevent overlapping TTS calls

def generate_audio(text, avatar_id=0):

    print("TTS FUNCTION CALLED")

    with tts_lock:
        try:
            engine = pyttsx3.init()
            voices = engine.getProperty('voices')

            # -----------------------------
            # Voice Detection
            # -----------------------------
            female_voice = None
            male_voice = None

            for v in voices:
                name = v.name.lower()

                if "zira" in name or "female" in name:
                    female_voice = v.id
                elif "david" in name or "male" in name:
                    male_voice = v.id

            # -----------------------------
            # Fallbacks
            # -----------------------------
            if female_voice is None:
                female_voice = voices[0].id

            if male_voice is None:
                male_voice = voices[-1].id

            # -----------------------------
            # Voice Mapping (4 avatars)
            # -----------------------------
            voice_map = [female_voice, male_voice, female_voice, male_voice]
            selected_voice = voice_map[avatar_id % len(voice_map)]

            engine.setProperty('voice', selected_voice)
            engine.setProperty('rate', 150)

            # ----------------------------- File Naming--------------------
            filename = f"output_{int(time.time() * 1000)}.wav"


            if not unity_project_path:
                raise ValueError("UNITY_PROJECT_PATH is not configured")

            unity_audio_path = os.path.join(
                unity_project_path,
                "Assets",
                "StreamingAssets",
                "audio"
            )
            #unity_audio_path = r"D:\Unity projects\VantageVR_current\VantageVR\Assets\StreamingAssets\audio"

            # Ensure folder exists
            os.makedirs(unity_audio_path, exist_ok=True)

            filepath = os.path.join(unity_audio_path, filename)

            print("Saving audio to:", filepath)

            # -----------------------------
            # Generate Audio
            # -----------------------------
            engine.save_to_file(text, filepath)
            engine.runAndWait()
            engine.stop()

            # -----------------------------
            # Verify file exists
            # -----------------------------
            if not os.path.exists(filepath):
                raise Exception("Audio file was not created!")

            print("TTS SUCCESS:", filename)

            # Return BOTH (important for Unity)
            return filename

        except Exception as e:
            print("========== TTS ERROR ==========")
            traceback.print_exc()
            print("================================")
            raise e