package com.example.mazegame

data class Cell(val col: Int, val row: Int) {
    var topWall = true
    var leftWall = true
    var bottomWall = true
    var rightWall = true
    var visited = false
}