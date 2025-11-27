package com.example.mazegame

import android.os.Bundle
import android.util.Log
import android.widget.Button
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.example.mazegame.R
import kotlin.concurrent.thread // Для запуску в фоні

class MainActivity : AppCompatActivity() {

    companion object {
        private const val TAG = "MainActivityLog"
    }

    private lateinit var mazeView: MazeView
    private var isSolving = false // Щоб не натиснути кнопку двічі

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        // Знаходимо View за ID
        mazeView = findViewById(R.id.mazeView)
        val btnSolve = findViewById<Button>(R.id.btnSolve)

        btnSolve.setOnClickListener {
            if (!isSolving) {
                solveMaze()
            }
        }

        Log.d(TAG, "Application started")
    }

    private fun solveMaze() {
        val path = mazeView.gameManager.findPath()

        if (path.isEmpty()) {
            Toast.makeText(this, "Шлях не знайдено (або ви вже на фініші)", Toast.LENGTH_SHORT).show()
            return
        }

        isSolving = true

        // Запускаємо анімацію в окремому потоці, щоб не "заморозити" екран
        thread {
            for (cell in path) {
                // Оновлюємо позицію гравця
                mazeView.gameManager.player = cell

                // Просимо Android перемалювати екран (обов'язково через runOnUiThread)
                runOnUiThread {
                    mazeView.invalidate()
                }

                // Пауза 100 мілісекунд між кроками (швидкість руху)
                Thread.sleep(100)
            }

            // Коли дійшли до кінця
            runOnUiThread {
                isSolving = false
                showWinMessage()
            }
        }
    }

    fun showWinMessage() {
        Log.i(TAG, "Showing win message toast")
        Toast.makeText(this, getString(R.string.win_message), Toast.LENGTH_LONG).show()
    }
}