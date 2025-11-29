package com.example.mazegame

import java.util.Stack
import java.util.Random // Використовуємо Java Random для фіксації рівнів

class GameManager(val cols: Int, val rows: Int) {
    val cells = Array(cols) { col -> Array(rows) { row -> Cell(col, row) } }
    var player = cells[0][0]
    private val exit = cells[cols - 1][rows - 1]

    // Змінна для генератора
    private var random = Random()

    // --- ЗМІНА: Метод для старту гри з конкретним seed ---
    fun generateMap(seed: Long?) {
        // Скидаємо клітинки
        for (x in 0 until cols) {
            for (y in 0 until rows) {
                cells[x][y] = Cell(x, y)
            }
        }
        player = cells[0][0]

        // Якщо seed передали - використовуємо його (рівень буде фіксований)
        // Якщо null - генеруємо випадковий час (рівень буде випадковий)
        if (seed != null) {
            random = Random(seed)
        } else {
            random = Random()
        }

        generateMazeAlgorithm()
    }

    // Перейменував старий метод, щоб не плутатись
    private fun generateMazeAlgorithm() {
        val stack = Stack<Cell>()
        var current = cells[0][0]
        current.visited = true

        do {
            val next = getRandomUnvisitedNeighbor(current)
            if (next != null) {
                removeWall(current, next)
                stack.push(current)
                current = next
                current.visited = true
            } else {
                current = stack.pop()
            }
        } while (stack.isNotEmpty())
    }

    private fun getRandomUnvisitedNeighbor(cell: Cell): Cell? {
        val neighbors = ArrayList<Cell>()
        // Тут замінили Random.nextInt на random.nextInt (наш об'єкт)
        if (cell.col > 0 && !cells[cell.col - 1][cell.row].visited) neighbors.add(cells[cell.col - 1][cell.row])
        if (cell.col < cols - 1 && !cells[cell.col + 1][cell.row].visited) neighbors.add(cells[cell.col + 1][cell.row])
        if (cell.row > 0 && !cells[cell.col][cell.row - 1].visited) neighbors.add(cells[cell.col][cell.row - 1])
        if (cell.row < rows - 1 && !cells[cell.col][cell.row + 1].visited) neighbors.add(cells[cell.col][cell.row + 1])

        return if (neighbors.isNotEmpty()) neighbors[random.nextInt(neighbors.size)] else null
    }

    // ... (Решта методів: removeWall, movePlayer, isWin, findPath - ЗАЛИШАЮТЬСЯ БЕЗ ЗМІН) ...

    private fun removeWall(current: Cell, next: Cell) {
        if (current.col == next.col && current.row == next.row + 1) {
            current.topWall = false; next.bottomWall = false
        }
        if (current.col == next.col && current.row == next.row - 1) {
            current.bottomWall = false; next.topWall = false
        }
        if (current.col == next.col + 1 && current.row == next.row) {
            current.leftWall = false; next.rightWall = false
        }
        if (current.col == next.col - 1 && current.row == next.row) {
            current.rightWall = false; next.leftWall = false
        }
    }

    fun movePlayer(direction: Direction): Boolean {
        return when (direction) {
            Direction.UP -> {
                if (!player.topWall) { player = cells[player.col][player.row - 1]; true } else false
            }
            Direction.DOWN -> {
                if (!player.bottomWall) { player = cells[player.col][player.row + 1]; true } else false
            }
            Direction.LEFT -> {
                if (!player.leftWall) { player = cells[player.col - 1][player.row]; true } else false
            }
            Direction.RIGHT -> {
                if (!player.rightWall) { player = cells[player.col + 1][player.row]; true } else false
            }
        }
    }

    fun isWin(): Boolean = player == exit

    fun findPath(): List<Cell> {
        val start = player
        val target = cells[cols - 1][rows - 1]
        val queue = java.util.LinkedList<Cell>()
        queue.add(start)
        val cameFrom = HashMap<Cell, Cell?>()
        cameFrom[start] = null
        val visited = HashSet<Cell>()
        visited.add(start)
        while (queue.isNotEmpty()) {
            val current = queue.poll()
            if (current == target) break
            if (!current.topWall) { val next = cells[current.col][current.row - 1]; if (!visited.contains(next)) { queue.add(next); visited.add(next); cameFrom[next] = current }}
            if (!current.bottomWall) { val next = cells[current.col][current.row + 1]; if (!visited.contains(next)) { queue.add(next); visited.add(next); cameFrom[next] = current }}
            if (!current.leftWall) { val next = cells[current.col - 1][current.row]; if (!visited.contains(next)) { queue.add(next); visited.add(next); cameFrom[next] = current }}
            if (!current.rightWall) { val next = cells[current.col + 1][current.row]; if (!visited.contains(next)) { queue.add(next); visited.add(next); cameFrom[next] = current }}
        }
        val path = ArrayList<Cell>()
        var curr: Cell? = target
        while (curr != null && curr != start) {
            path.add(curr)
            curr = cameFrom[curr]
        }
        path.reverse()
        return path
    }
}