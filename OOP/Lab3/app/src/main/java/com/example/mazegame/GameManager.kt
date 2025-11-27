package com.example.mazegame

import java.util.Stack
import kotlin.random.Random

class GameManager( val cols: Int, val rows: Int) {
    val cells = Array(cols) { col -> Array(rows) { row -> Cell(col, row) } }
    var player = cells[0][0]
    private val exit = cells[cols - 1][rows - 1]

    init {
        generateMaze()
    }

    private fun generateMaze() {
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

        // Перевірка сусідів (Left, Right, Top, Bottom)
        if (cell.col > 0 && !cells[cell.col - 1][cell.row].visited) neighbors.add(cells[cell.col - 1][cell.row])
        if (cell.col < cols - 1 && !cells[cell.col + 1][cell.row].visited) neighbors.add(cells[cell.col + 1][cell.row])
        if (cell.row > 0 && !cells[cell.col][cell.row - 1].visited) neighbors.add(cells[cell.col][cell.row - 1])
        if (cell.row < rows - 1 && !cells[cell.col][cell.row + 1].visited) neighbors.add(cells[cell.col][cell.row + 1])

        return if (neighbors.isNotEmpty()) neighbors[Random.nextInt(neighbors.size)] else null
    }

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
        val target = cells[cols - 1][rows - 1] // Вихід

        // Черга для перевірки клітинок
        val queue = java.util.LinkedList<Cell>()
        queue.add(start)

        // Словник, щоб пам'ятати, звідки ми прийшли в кожну клітинку
        // (щоб потім відновити шлях назад)
        val cameFrom = HashMap<Cell, Cell?>()
        cameFrom[start] = null

        val visited = HashSet<Cell>()
        visited.add(start)

        while (queue.isNotEmpty()) {
            val current = queue.poll()

            // Якщо дійшли до фінішу - зупиняємось
            if (current == target) {
                break
            }

            // Перевіряємо сусідів
            // (Логіка: якщо немає стіни І сусід не відвіданий -> додаємо в чергу)

            // ВЕРХ
            if (!current.topWall) {
                val next = cells[current.col][current.row - 1]
                if (!visited.contains(next)) {
                    queue.add(next)
                    visited.add(next)
                    cameFrom[next] = current
                }
            }
            // НИЗ
            if (!current.bottomWall) {
                val next = cells[current.col][current.row + 1]
                if (!visited.contains(next)) {
                    queue.add(next)
                    visited.add(next)
                    cameFrom[next] = current
                }
            }
            // ЛІВО
            if (!current.leftWall) {
                val next = cells[current.col - 1][current.row]
                if (!visited.contains(next)) {
                    queue.add(next)
                    visited.add(next)
                    cameFrom[next] = current
                }
            }
            // ПРАВО
            if (!current.rightWall) {
                val next = cells[current.col + 1][current.row]
                if (!visited.contains(next)) {
                    queue.add(next)
                    visited.add(next)
                    cameFrom[next] = current
                }
            }
        }

        // Відновлюємо шлях від фінішу до старту
        val path = ArrayList<Cell>()
        var curr: Cell? = target
        while (curr != null && curr != start) {
            path.add(curr)
            curr = cameFrom[curr]
        }
        // Перевертаємо, щоб шлях йшов від старту до фінішу
        path.reverse()
        return path
    }
}