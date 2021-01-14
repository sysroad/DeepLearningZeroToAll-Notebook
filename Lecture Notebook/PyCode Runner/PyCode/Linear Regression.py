import tensorflow as tf
import matplotlib.pyplot as plt

X = tf.constant([1, 2, 3], dtype=tf.float32)
Y = tf.constant([1, 2, 3], dtype=tf.float32)

W = tf.Variable(initial_value=0, dtype=tf.float32)

# prepare list
W_val = []
cost_val = []

learning_rate = 0.1

for i in range(-30, 50):
    W = float(i) * learning_rate

    cost = tf.reduce_mean(
        tf.square(tf.subtract(tf.multiply(X, W), Y))
    )

    W_val.append(W)
    cost_val.append(cost)

plt.plot(W_val, cost_val)
plt.show()
