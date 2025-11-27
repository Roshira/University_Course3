package com.example.mazegame

import android.content.Context // Потрібен для SharedPreferences
import android.os.Bundle
import android.util.Log
import android.widget.Button
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.app.AppCompatDelegate
import androidx.core.os.LocaleListCompat
import kotlin.concurrent.thread

class MainActivity : AppCompatActivity() {

    companion object {
        private const val TAG = "MainActivityLog"
    }

    private lateinit var mazeView: MazeView
    @Volatile private var isSolving = false

    // --- НОВЕ: Зберігаємо поточний seed, щоб знати, який рівень ми проходимо ---
    private var currentSeed: Long = -1

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        mazeView = findViewById(R.id.mazeView)
        val btnSolve = findViewById<Button>(R.id.btnSolve)
        val btnRestart = findViewById<Button>(R.id.btnRestart)
        val btnLang = findViewById<Button>(R.id.btnLang)
        val btnBackToMenu = findViewById<Button>(R.id.btnBackToMenu)

        // Отримуємо Seed і зберігаємо в змінну класу
        currentSeed = intent.getLongExtra("SEED", -1)

        if (currentSeed != -1L) {
            mazeView.startNewGame(currentSeed)
        } else {
            mazeView.startNewGame(null)
        }

        btnBackToMenu.setOnClickListener {
            isSolving = false
            finish()
        }

        btnSolve.setOnClickListener { if (!isSolving) solveMaze() }
        btnRestart.setOnClickListener { restartGame() }
        btnLang.setOnClickListener { toggleLanguage() }

        Log.d(TAG, "MainActivity initialized")
    }

    // ... (toggleLanguage, restartGame, solveMaze - залишаються без змін) ...
    private fun toggleLanguage() {
        val currentLocale = AppCompatDelegate.getApplicationLocales()[0]
        val newLocale = if (currentLocale?.language == "uk") LocaleListCompat.forLanguageTags("en") else LocaleListCompat.forLanguageTags("uk")
        AppCompatDelegate.setApplicationLocales(newLocale)
    }

    private fun restartGame() {
        isSolving = false
        mazeView.reset()
    }

    private fun solveMaze() {
        val path = mazeView.gameManager.findPath()
        if (path.isEmpty()) return
        isSolving = true
        thread {
            for (cell in path) {
                if (!isSolving) break
                mazeView.gameManager.player = cell
                runOnUiThread { mazeView.invalidate() }
                Thread.sleep(100)
            }
            if (isSolving) {
                runOnUiThread {
                    isSolving = false
                    showWinMessage()
                }
            }
        }
    }

    // --- ОНОВЛЕНИЙ МЕТОД ПЕРЕМОГИ ---
    fun showWinMessage() {
        Log.i(TAG, "User won!")
        Toast.makeText(this, getString(R.string.win_message), Toast.LENGTH_LONG).show()

        // Якщо це був фіксований рівень (seed != -1), зберігаємо прогрес
        if (currentSeed != -1L) {
            saveLevelProgress(currentSeed)
        }
    }

    private fun saveLevelProgress(seed: Long) {
        // Відкриваємо "файл" налаштувань з назвою "GameProgress"
        val sharedPref = getSharedPreferences("GameProgress", Context.MODE_PRIVATE)
        val editor = sharedPref.edit()

        // Записуємо: "Level_1111" = true (пройдено)
        // Ми використовуємо сам seed як частину ключа, це дуже зручно
        editor.putBoolean("LEVEL_$seed", true)
        editor.apply() // Зберігаємо

        Log.i(TAG, "Progress saved for seed: $seed")
    }
}