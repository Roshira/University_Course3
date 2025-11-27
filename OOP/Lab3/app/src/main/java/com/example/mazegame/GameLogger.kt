package com.example.mazegame

import android.util.Log

/**
 * Власний логер для централізованого керування повідомленнями.
 * Дозволяє легко вимкнути всі логи в додатку одним перемикачем.
 */
object GameLogger {

    // BuildConfig.DEBUG автоматично стає true, коли ви запускаєте через Android Studio,
    // і false, коли створюєте файл для магазину (Release).
    private val IS_LOGGING_ENABLED = BuildConfig.DEBUG

    fun d(tag: String, message: String) {
        if (IS_LOGGING_ENABLED) {
            Log.d(tag, "🎮 $message") // Додаємо смайлик, щоб легко бачити свої логи
        }
    }

    fun i(tag: String, message: String) {
        if (IS_LOGGING_ENABLED) {
            Log.i(tag, "ℹ️ $message")
        }
    }

    fun e(tag: String, message: String, error: Throwable? = null) {
        if (IS_LOGGING_ENABLED) {
            Log.e(tag, "🔥 $message", error)
        }
    }
}