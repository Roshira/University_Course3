package com.example.mazegame

import android.content.Context
import android.content.Intent
import android.graphics.Color
import android.os.Bundle
import android.widget.Button
import androidx.appcompat.app.AppCompatActivity

class LevelSelectActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_levels)

        // Налаштовуємо кнопки і одразу перевіряємо, чи пройдені рівні
        setupLevelButton(R.id.btnLvl1, 1111)
        setupLevelButton(R.id.btnLvl2, 2222)
        setupLevelButton(R.id.btnLvl3, 3333)
        setupLevelButton(R.id.btnLvl4, 4444)
        setupLevelButton(R.id.btnLvl5, 5555)

        findViewById<Button>(R.id.btnBack).setOnClickListener {
            finish()
        }
    }

    // Цей метод викликається щоразу, коли ми повертаємось на цей екран
    // (наприклад, після проходження рівня)
    override fun onResume() {
        super.onResume()
        // Оновлюємо кольори кнопок, бо статус міг змінитися
        updateButtonStatus(R.id.btnLvl1, 1111)
        updateButtonStatus(R.id.btnLvl2, 2222)
        updateButtonStatus(R.id.btnLvl3, 3333)
        updateButtonStatus(R.id.btnLvl4, 4444)
        updateButtonStatus(R.id.btnLvl5, 5555)
    }

    private fun setupLevelButton(btnId: Int, seed: Long) {
        findViewById<Button>(btnId).setOnClickListener {
            val intent = Intent(this, MainActivity::class.java)
            intent.putExtra("SEED", seed)
            startActivity(intent)
        }
    }

    private fun updateButtonStatus(btnId: Int, seed: Long) {
        val btn = findViewById<Button>(btnId)

        // Читаємо з пам'яті
        val sharedPref = getSharedPreferences("GameProgress", Context.MODE_PRIVATE)
        val isCompleted = sharedPref.getBoolean("LEVEL_$seed", false)

        if (isCompleted) {
            // Якщо пройдено - робимо кнопку зеленою і додаємо галочку
            btn.setBackgroundColor(Color.parseColor("#4CAF50")) // Зелений

            // Щоб не дублювати галочки, перевіряємо чи вона вже є
            if (!btn.text.toString().contains("✅")) {
                btn.text = "${btn.text} ✅"
            }
        }
    }
}