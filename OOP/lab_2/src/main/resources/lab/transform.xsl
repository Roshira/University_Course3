<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:c="http://www.example.com/candies">
    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="/">
        <CategorizedCandies>
            <!-- Групування за типом цукерки -->
            <xsl:for-each select="//c:Candy[generate-id() = generate-id(key('type-key', c:Type)[1])]">
                <xsl:sort select="c:Type"/>
                <xsl:variable name="currentType" select="c:Type"/>

                <CandyGroup type="{$currentType}">
                    <xsl:for-each select="//c:Candy[c:Type = $currentType]">
                        <Candy>
                            <Name><xsl:value-of select="c:Name"/></Name>
                            <Energy><xsl:value-of select="c:Energy"/></Energy>
                            <Production><xsl:value-of select="@production"/></Production>
                        </Candy>
                    </xsl:for-each>
                </CandyGroup>
            </xsl:for-each>

            <!-- Групування за виробником -->
            <xsl:for-each select="//c:Candy[generate-id() = generate-id(key('production-key', @production)[1])]">
                <xsl:sort select="@production"/>
                <xsl:variable name="currentProduction" select="@production"/>

                <ProductionGroup production="{$currentProduction}">
                    <xsl:for-each select="//c:Candy[@production = $currentProduction]">
                        <Candy>
                            <Name><xsl:value-of select="c:Name"/></Name>
                            <Type><xsl:value-of select="c:Type"/></Type>
                            <Energy><xsl:value-of select="c:Energy"/></Energy>
                        </Candy>
                    </xsl:for-each>
                </ProductionGroup>
            </xsl:for-each>
        </CategorizedCandies>
    </xsl:template>

    <xsl:key name="type-key" match="c:Candy" use="c:Type"/>
    <xsl:key name="production-key" match="c:Candy" use="@production"/>
</xsl:stylesheet>