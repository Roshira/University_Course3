<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:c="http://www.example.com/candies">
    <xsl:output method="html" indent="yes"/>

    <xsl:template match="/">
        <html>
            <head>
                <title>Candies List</title>
                <style>
                    table, th, td { border: 1px solid black; border-collapse: collapse; }
                    th, td { padding: 8px; text-align: left;}
                </style>
            </head>
            <body>
                <h2>Candies Catalogue</h2>
                <table>
                    <tr style="background-color:#f2f2f2;">
                        <th>ID</th>
                        <th>Name</th>
                        <th>Energy (kcal)</th>
                        <th>Type</th>
                        <th>Production</th>
                    </tr>
                    <xsl:apply-templates select="c:Candies/c:Candy"/>
                </table>
            </body>
        </html>
    </xsl:template>

    <xsl:template match="c:Candy">
        <tr>
            <td><xsl:value-of select="@id"/></td>
            <td><xsl:value-of select="c:Name"/></td>
            <td><xsl:value-of select="c:Energy"/></td>
            <td><xsl:value-of select="c:Type"/></td>
            <td><xsl:value-of select="@production"/></td>
        </tr>
    </xsl:template>
</xsl:stylesheet>