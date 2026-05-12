import os
import numpy as np
from tensorflow.keras.models import load_model
from tensorflow.keras.preprocessing.image import load_img, img_to_array
from tkinter import Tk, Label
from tkinterdnd2 import DND_FILES, TkinterDnD

# Etiketler
labels = {0: 'Healthy', 1: 'Unhealthy'}

# Modeli yükle
model = load_model('model.h5')
print("Model loaded.")

# Görseli hazırlama
def preprocess_image(image_path, target_size=(225, 225)):
    img = load_img(image_path, target_size=target_size)
    x = img_to_array(img)
    x = x.astype('float32') / 255.
    x = np.expand_dims(x, axis=0)
    return x



# Güncellenmiş tahmin fonksiyonu
def predict_image(file_path):
    x = preprocess_image(file_path)
    predictions = model.predict(x)
    predicted_index = np.argmax(predictions)
    predicted_label = labels[predicted_index]
    confidence = float(predictions[0][predicted_index]) * 100
    result_label.config(
        text=f"'{os.path.basename(file_path)}': {predicted_label} ({confidence:.2f}%)"
    )


# GUI başlat
app = TkinterDnD.Tk()
app.title("Leaf Image Prediction")
app.geometry("700x500")  # Boyutu büyüttük
app.configure(bg="#293133")


# Sürükle-bırak alanı
drop_label = Label(
    app,
    text="Drag the leaf image into this area for prediction",
    width=55,
    height=13,
    bg='lightgray',
    relief='ridge',
    font=("Arial", 12)
)
drop_label.pack(padx=20, pady=(40, 10))  # Üstten biraz boşluk bıraktık

# Tahmin sonucunu gösterecek alan
result_label = Label(
    app,
    text="",
    fg="white",
    bg="#293133",
    font=("Arial", 14)
)
result_label.pack(pady=(10, 0))  # Drop alanı altına biraz boşlukla yerleştirildi

# Sürükleme olayı
def drop(event):
    file_path = event.data.strip('{}')
    if os.path.isfile(file_path):
        predict_image(file_path)

drop_label.drop_target_register(DND_FILES)
drop_label.dnd_bind('<<Drop>>', drop)


app.mainloop()
