import matplotlib.pyplot as plt
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Conv2D, MaxPooling2D, Flatten, Dense


TRAIN_DIR = r"Train_Dataset_Location"
VAL_DIR = r"Validation_Dataset_Location"

# ImageDataGenerator
train_datagen = ImageDataGenerator(
    rescale=1.0/255,
    shear_range=0.2,
    zoom_range=0.2,
    horizontal_flip=True
)

val_datagen = ImageDataGenerator(rescale=1.0/255)

# Batch generator
train_generator = train_datagen.flow_from_directory(
    TRAIN_DIR,
    target_size=(225, 225),
    batch_size=32,
    class_mode='categorical'
)

val_generator = val_datagen.flow_from_directory(
    VAL_DIR,
    target_size=(225, 225),
    batch_size=32,
    class_mode='categorical'
)

# Model description
model = Sequential()
model.add(Conv2D(32, (3, 3), activation='relu', input_shape=(225, 225, 3)))
model.add(MaxPooling2D(pool_size=(2, 2)))

model.add(Conv2D(64, (3, 3), activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2)))

model.add(Flatten())
model.add(Dense(64, activation='relu'))
model.add(Dense(2, activation='softmax'))  # Sadece 2 sınıf

model.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])

# Training
history = model.fit(
    train_generator,
    epochs=10,
    validation_data=val_generator
)

# Save
model.save("model.h5")
print("✅ Eğitim tamamlandı ve model.h5 kaydedildi.")

# Results
plt.plot(history.history['accuracy'], label='Training')
plt.plot(history.history['val_accuracy'], label='Validation')
plt.title('Model Accuracy Curve')
plt.xlabel('Epoch')
plt.ylabel('Accuracy')
plt.legend()
plt.grid(True)
plt.show()
