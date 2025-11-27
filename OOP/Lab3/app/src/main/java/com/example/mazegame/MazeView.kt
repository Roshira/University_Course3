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
import kotlin.math.abs
import kotlin.math.max
import kotlin.math.min

private const val TAG = "MazeView"

class MazeView @JvmOverloads constructor(
    context: Context,
    attrs: AttributeSet? = null,
    defStyleAttr: Int = 0
) : View(context, attrs, defStyleAttr) {

    private val wallPaint = Paint().apply {
        color = Color.BLACK
        strokeWidth = 4f
        style = Paint.Style.STROKE
        strokeCap = Paint.Cap.ROUND
    }

    private var exitBitmap: Bitmap? = null
    private var playerBitmap: Bitmap? = null
    private var floorBitmap: Bitmap? = null

    private var cellSize = 0f
    private var hMargin = 0f
    private var vMargin = 0f

    // Налаштування камери
    private val ZOOM_FACTOR = 2.0f // Трохи зменшив зум для зручності
    private var currentCameraX = 0f
    private var currentCameraY = 0f

    // --- НОВІ ЗМІННІ ДЛЯ ПЛАВНОСТІ ---
    // Це "візуальні" координати (Float), які плавно наздоганяють реальні (Int)
    private var visualPlayerCol = 0f
    private var visualPlayerRow = 0f

    // Швидкість анімації (0.1 = повільно/плавно, 0.3 = швидко)
    private val SMOOTH_SPEED = 0.2f
    // ---------------------------------

    private val COLS = 13
    private val ROWS = 20

    val gameManager = GameManager(COLS, ROWS)

    private var currentSeed: Long? = null

    init {
        GameLogger.d(TAG, "MazeView initialized")
        exitBitmap = BitmapFactory.decodeResource(resources, R.drawable.ic_exit)
        playerBitmap = BitmapFactory.decodeResource(resources, R.drawable.ic_player)
        floorBitmap = BitmapFactory.decodeResource(resources, R.drawable.maze_floor)

        // За замовчуванням запускаємо випадкову гру
        gameManager.generateMap(null)
    }
    fun startNewGame(seed: Long?) {
        currentSeed = seed
        gameManager.generateMap(seed)

        // Скидаємо візуалізацію
        visualPlayerCol = 0f
        visualPlayerRow = 0f
        currentCameraX = 0f
        currentCameraY = 0f

        invalidate()
    }
    fun reset() {
        // Перезапускаємо з ТИМ ЖЕ seed, що був (щоб рівень не змінився)
        startNewGame(currentSeed)
    }

    override fun onSizeChanged(w: Int, h: Int, oldw: Int, oldh: Int) {
        super.onSizeChanged(w, h, oldw, oldh)
        if (w > 0 && h > 0) {
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

        // --- ЛОГІКА ПЛАВНОСТІ (LERP - Linear Interpolation) ---
        val targetCol = gameManager.player.col.toFloat()
        val targetRow = gameManager.player.row.toFloat()

        // Формула: Поточне = Поточне + (Ціль - Поточне) * Швидкість
        // Це змушує visualPlayerCol плавно наближатися до targetCol
        visualPlayerCol += (targetCol - visualPlayerCol) * SMOOTH_SPEED
        visualPlayerRow += (targetRow - visualPlayerRow) * SMOOTH_SPEED

        // Перевіряємо, чи ми все ще рухаємось. Якщо різниця мала - зупиняємось.
        val isMoving = abs(targetCol - visualPlayerCol) > 0.01f || abs(targetRow - visualPlayerRow) > 0.01f
        // ------------------------------------------------------

        val viewW = width.toFloat()
        val viewH = height.toFloat()
        val totalMazeW = COLS * cellSize
        val totalMazeH = ROWS * cellSize

        // --- КАМЕРА (Тепер прив'язана до visualPlayerCol, тобто теж плавна) ---

        val playerCenterX = visualPlayerCol * cellSize + cellSize / 2
        val playerCenterY = visualPlayerRow * cellSize + cellSize / 2

        val targetCameraX = (viewW / 2) - (playerCenterX * ZOOM_FACTOR)
        val targetCameraY = (viewH / 2) - (playerCenterY * ZOOM_FACTOR)

        val maxCameraX = hMargin
        val minCameraX = viewW - hMargin - (totalMazeW * ZOOM_FACTOR)
        val maxCameraY = vMargin
        val minCameraY = viewH - vMargin - (totalMazeH * ZOOM_FACTOR)

        // Плавно рухаємо і камеру теж (можна використати ту ж інтерполяцію або жорстку прив'язку до плавного гравця)
        // Тут ми просто беремо координати, які вже залежать від плавного гравця
        currentCameraX = max(minCameraX, min(targetCameraX, maxCameraX))
        currentCameraY = max(minCameraY, min(targetCameraY, maxCameraY))

        canvas.save()
        canvas.translate(currentCameraX, currentCameraY)
        canvas.scale(ZOOM_FACTOR, ZOOM_FACTOR)

        // --- МАЛЮВАННЯ ---

        // 1. Підлога
        if (floorBitmap != null) {
            val floorRect = Rect(0, 0, totalMazeW.toInt(), totalMazeH.toInt())
            canvas.drawBitmap(floorBitmap!!, null, floorRect, null)
        }

        // 2. Стіни
        for (x in 0 until COLS) {
            for (y in 0 until ROWS) {
                val cell = gameManager.cells[x][y]
                val currentX = x * cellSize
                val currentY = y * cellSize

                // Оптимізація: малюємо стіни тільки якщо вони потрапляють в кадр (приблизно)
                // Але для простоти малюємо всі
                if (cell.topWall) canvas.drawLine(currentX, currentY, currentX + cellSize, currentY, wallPaint)
                if (cell.leftWall) canvas.drawLine(currentX, currentY, currentX, currentY + cellSize, wallPaint)
                if (cell.bottomWall) canvas.drawLine(currentX, currentY + cellSize, currentX + cellSize, currentY + cellSize, wallPaint)
                if (cell.rightWall) canvas.drawLine(currentX + cellSize, currentY, currentX + cellSize, currentY + cellSize, wallPaint)
            }
        }

        val margin = cellSize / 10
        val size = cellSize - 2 * margin

        // 3. Вихід
        val exitX = (COLS - 1) * cellSize + margin
        val exitY = (ROWS - 1) * cellSize + margin
        if (exitBitmap != null) {
            val dstRect = Rect(exitX.toInt(), exitY.toInt(), (exitX + size).toInt(), (exitY + size).toInt())
            canvas.drawBitmap(exitBitmap!!, null, dstRect, null)
        } else {
            val exitPaint = Paint().apply { color = Color.GREEN }
            canvas.drawRect(exitX, exitY, exitX + size, exitY + size, exitPaint)
        }

        // 4. Гравець (Використовуємо ПЛАВНІ координати visualPlayerCol/Row)
        val playerX = visualPlayerCol * cellSize + margin
        val playerY = visualPlayerRow * cellSize + margin

        if (playerBitmap != null) {
            val playerRect = Rect(playerX.toInt(), playerY.toInt(), (playerX + size).toInt(), (playerY + size).toInt())
            canvas.drawBitmap(playerBitmap!!, null, playerRect, null)
        } else {
            val playerPaint = Paint().apply { color = Color.RED }
            canvas.drawRect(playerX, playerY, playerX + size, playerY + size, playerPaint)
        }

        canvas.restore()

        // --- КЛЮЧОВИЙ МОМЕНТ ---
        // Якщо ми ще рухаємось (анімація не закінчилась), просимо Android
        // перемалювати екран у наступному кадрі. Це створює цикл анімації.
        if (isMoving) {
            invalidate()
        }
    }

    override fun onTouchEvent(event: MotionEvent): Boolean {
        if (event.action == MotionEvent.ACTION_DOWN) return true
        if (event.action == MotionEvent.ACTION_MOVE) {
            // Конвертація координат з урахуванням камери
            val touchX = (event.x - currentCameraX) / ZOOM_FACTOR
            val touchY = (event.y - currentCameraY) / ZOOM_FACTOR

            // Використовуємо visualPlayerCol для центру, щоб свайп працював відносно поточної видимої позиції
            val playerCenterX = visualPlayerCol * cellSize + cellSize / 2
            val playerCenterY = visualPlayerRow * cellSize + cellSize / 2

            val dx = touchX - playerCenterX
            val dy = touchY - playerCenterY

            val threshold = cellSize / 3

            // Перевірка, щоб не спамити рухи, поки йде анімація
            // Якщо візуальна позиція ще далеко від цільової, ігноруємо новий свайп
            val isAnimationFinished = abs(gameManager.player.col - visualPlayerCol) < 0.1f &&
                    abs(gameManager.player.row - visualPlayerRow) < 0.1f

            if (isAnimationFinished) {
                if (abs(dx) > threshold || abs(dy) > threshold) {
                    if (abs(dx) > abs(dy)) {
                        if (dx > 0) movePlayer(Direction.RIGHT) else movePlayer(Direction.LEFT)
                    } else {
                        if (dy > 0) movePlayer(Direction.DOWN) else movePlayer(Direction.UP)
                    }
                }
            }
            return true
        }
        return super.onTouchEvent(event)
    }

    private fun movePlayer(direction: Direction) {
        if (gameManager.movePlayer(direction)) {
            // invalidate запустить onDraw, який почне анімацію LERP
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