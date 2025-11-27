package com.example.mazegame

import android.content.Context
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Paint
import android.graphics.Rect
import android.util.AttributeSet
import android.util.Log
import android.view.MotionEvent
import android.view.View

private const val TAG = "MazeView"

class MazeView @JvmOverloads constructor(
    context: Context,
    attrs: AttributeSet? = null,
    defStyleAttr: Int = 0
) : View(context, attrs, defStyleAttr) {

    // Змінив товщину стін на 3f, щоб на великій карті вони виглядали акуратніше
    private val wallPaint = Paint().apply {
        color = Color.BLACK
        strokeWidth = 3f
        style = Paint.Style.STROKE
        strokeCap = Paint.Cap.ROUND
    }

    // Змінні для картинок
    private var exitBitmap: Bitmap? = null
    private var playerBitmap: Bitmap? = null // <--- НОВЕ: Картинка гравця

    private var cellSize = 0f
    private var hMargin = 0f
    private var vMargin = 0f

    // --- НОВЕ: ЗБІЛЬШЕНИЙ РОЗМІР ЛАБІРИНТУ ---
    // Було 7x10, стало 13x20. Можете ставити будь-які числа!
    private val COLS = 13
    private val ROWS = 20

    val gameManager = GameManager(COLS, ROWS)

    init {
        Log.d(TAG, "MazeView initialized")

        // Завантажуємо картинку виходу (якщо є)
        exitBitmap = BitmapFactory.decodeResource(resources, R.drawable.ic_exit)

        // --- НОВЕ: Завантажуємо картинку гравця ---
        // Переконайтеся, що файл ic_player.png є у папці drawable!
        playerBitmap = BitmapFactory.decodeResource(resources, R.drawable.ic_player)
    }

    override fun onSizeChanged(w: Int, h: Int, oldw: Int, oldh: Int) {
        super.onSizeChanged(w, h, oldw, oldh)

        if (w > 0 && h > 0) {
            // Розрахунок розміру клітинки, щоб лабіринт вліз у екран
            if (w.toFloat() / h < COLS.toFloat() / ROWS) {
                cellSize = w / (COLS + 1).toFloat()
            } else {
                cellSize = h / (ROWS + 1).toFloat()
            }
            hMargin = (w - COLS * cellSize) / 2
            vMargin = (h - ROWS * cellSize) / 2
        }
    }

    override fun onDraw(canvas: Canvas) {
        super.onDraw(canvas)

        if (cellSize <= 0) return

        canvas.translate(hMargin, vMargin)

        // 1. Малюємо стіни
        for (x in 0 until COLS) {
            for (y in 0 until ROWS) {
                val cell = gameManager.cells[x][y]
                val currentX = x * cellSize
                val currentY = y * cellSize

                if (cell.topWall)
                    canvas.drawLine(currentX, currentY, currentX + cellSize, currentY, wallPaint)
                if (cell.leftWall)
                    canvas.drawLine(currentX, currentY, currentX, currentY + cellSize, wallPaint)
                if (cell.bottomWall)
                    canvas.drawLine(currentX, currentY + cellSize, currentX + cellSize, currentY + cellSize, wallPaint)
                if (cell.rightWall)
                    canvas.drawLine(currentX + cellSize, currentY, currentX + cellSize, currentY + cellSize, wallPaint)
            }
        }

        val margin = cellSize / 10

        // 2. Малюємо ВИХІД
        val exitX = (COLS - 1) * cellSize + margin
        val exitY = (ROWS - 1) * cellSize + margin
        val size = cellSize - 2 * margin

        if (exitBitmap != null) {
            val dstRect = Rect(exitX.toInt(), exitY.toInt(), (exitX + size).toInt(), (exitY + size).toInt())
            canvas.drawBitmap(exitBitmap!!, null, dstRect, null)
        } else {
            val exitPaint = Paint().apply { color = Color.GREEN }
            canvas.drawRect(exitX, exitY, exitX + size, exitY + size, exitPaint)
        }

        // 3. --- НОВЕ: Малюємо ГРАВЦЯ ---
        val playerX = gameManager.player.col * cellSize + margin
        val playerY = gameManager.player.row * cellSize + margin

        if (playerBitmap != null) {
            // Якщо картинка є - малюємо її
            val playerRect = Rect(playerX.toInt(), playerY.toInt(), (playerX + size).toInt(), (playerY + size).toInt())
            canvas.drawBitmap(playerBitmap!!, null, playerRect, null)
        } else {
            // Якщо картинки немає - малюємо старий червоний квадрат
            val playerPaint = Paint().apply { color = Color.RED }
            canvas.drawRect(playerX, playerY, playerX + size, playerY + size, playerPaint)
        }
    }

    override fun onTouchEvent(event: MotionEvent): Boolean {
        if (event.action == MotionEvent.ACTION_DOWN) return true
        if (event.action == MotionEvent.ACTION_MOVE) {
            val x = event.x
            val y = event.y
            val playerCenterX = hMargin + (gameManager.player.col + 0.5f) * cellSize
            val playerCenterY = vMargin + (gameManager.player.row + 0.5f) * cellSize

            val dx = x - playerCenterX
            val dy = y - playerCenterY

            // Оскільки клітинки стали меншими, робимо чутливість вищою (третина клітинки)
            val threshold = cellSize / 3

            if (kotlin.math.abs(dx) > threshold || kotlin.math.abs(dy) > threshold) {
                if (kotlin.math.abs(dx) > kotlin.math.abs(dy)) {
                    if (dx > 0) movePlayer(Direction.RIGHT) else movePlayer(Direction.LEFT)
                } else {
                    if (dy > 0) movePlayer(Direction.DOWN) else movePlayer(Direction.UP)
                }
            }
            return true
        }
        return super.onTouchEvent(event)
    }

    private fun movePlayer(direction: Direction) {
        if (gameManager.movePlayer(direction)) {
            invalidate()
            checkWin()
        }
    }

    private fun checkWin() {
        if (gameManager.isWin()) {
            (context as? MainActivity)?.showWinMessage()
        }
    }
}