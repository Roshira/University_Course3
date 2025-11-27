<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:c="http://www.example.com/candies">
    <xsl:output method="xml" indent="yes" encoding="UTF-8"/>

    <!-- Ключі для групування -->
    <xsl:key name="candy-by-type" match="c:Candy" use="c:Type"/>
    <xsl:key name="candy-by-production" match="c:Candy" use="@production"/>

    <xsl:template match="/">
        <CategorizedCandies xmlns="http://www.example.com/candies">
            <xsl:comment>Групування за типом цукерок</xsl:comment>

            <!-- Групування за типом -->
            <CandyTypes>
                <xsl:for-each select="c:Candies/c:Candy[generate-id() = generate-id(key('candy-by-type', c:Type)[1])]">
                    <xsl:sort select="c:Type"/>
                    <xsl:variable name="currentType" select="c:Type"/>

                    <CandyType type="{$currentType}">
                        <xsl:for-each select="key('candy-by-type', $currentType)">
                            <Candy>
                                <xsl:copy-of select="@id"/>
                                <xsl:copy-of select="@production"/>
                                <Name><xsl:value-of select="c:Name"/></Name>
                                <Energy><xsl:value-of select="c:Energy"/></Energy>
                                <Ingredients>
                                    <Water><xsl:value-of select="c:Ingredients/c:Water"/></Water>
                                    <Sugar><xsl:value-of select="c:Ingredients/c:Sugar"/></Sugar>
                                    <Fructose><xsl:value-of select="c:Ingredients/c:Fructose"/></Fructose>
                                    <xsl:if test="c:Ingredients/c:ChocolateType">
                                        <ChocolateType><xsl:value-of select="c:Ingredients/c:ChocolateType"/></ChocolateType>
                                    </xsl:if>
                                    <Vanillin><xsl:value-of select="c:Ingredients/c:Vanillin"/></Vanillin>
                                </Ingredients>
                                <Value>
                                    <Proteins><xsl:value-of select="c:Value/c:Proteins"/></Proteins>
                                    <Fats><xsl:value-of select="c:Value/c:Fats"/></Fats>
                                    <Carbohydrates><xsl:value-of select="c:Value/c:Carbohydrates"/></Carbohydrates>
                                </Value>
                            </Candy>
                        </xsl:for-each>
                    </CandyType>
                </xsl:for-each>
            </CandyTypes>

            <xsl:comment>Групування за виробником</xsl:comment>

            <!-- Групування за виробником -->
            <Productions>
                <xsl:for-each select="c:Candies/c:Candy[generate-id() = generate-id(key('candy-by-production', @production)[1])]">
                    <xsl:sort select="@production"/>
                    <xsl:variable name="currentProduction" select="@production"/>

                    <Production name="{$currentProduction}">
                        <xsl:for-each select="key('candy-by-production', $currentProduction)">
                            <Candy>
                                <xsl:copy-of select="@id"/>
                                <Name><xsl:value-of select="c:Name"/></Name>
                                <Type><xsl:value-of select="c:Type"/></Type>
                                <Energy><xsl:value-of select="c:Energy"/></Energy>
                                <xsl:if test="c:Ingredients/c:ChocolateType">
                                    <Chocolate><xsl:value-of select="c:Ingredients/c:ChocolateType"/></Chocolate>
                                </xsl:if>
                            </Candy>
                        </xsl:for-each>
                    </Production>
                </xsl:for-each>
            </Productions>

            <xsl:comment>Статистика за енергетичною цінністю</xsl:comment>

            <!-- Групування за енергетичною цінністю -->
            <EnergyGroups>
                <LowEnergy group="low" max="300">
                    <xsl:for-each select="c:Candies/c:Candy[c:Energy &lt;= 300]">
                        <Candy>
                            <xsl:copy-of select="@id"/>
                            <Name><xsl:value-of select="c:Name"/></Name>
                            <Energy><xsl:value-of select="c:Energy"/></Energy>
                            <Type><xsl:value-of select="c:Type"/></Type>
                        </Candy>
                    </xsl:for-each>
                </LowEnergy>

                <MediumEnergy group="medium" min="301" max="500">
                    <xsl:for-each select="c:Candies/c:Candy[c:Energy &gt; 300 and c:Energy &lt;= 500]">
                        <Candy>
                            <xsl:copy-of select="@id"/>
                            <Name><xsl:value-of select="c:Name"/></Name>
                            <Energy><xsl:value-of select="c:Energy"/></Energy>
                            <Type><xsl:value-of select="c:Type"/></Type>
                        </Candy>
                    </xsl:for-each>
                </MediumEnergy>

                <HighEnergy group="high" min="501">
                    <xsl:for-each select="c:Candies/c:Candy[c:Energy &gt; 500]">
                        <Candy>
                            <xsl:copy-of select="@id"/>
                            <Name><xsl:value-of select="c:Name"/></Name>
                            <Energy><xsl:value-of select="c:Energy"/></Energy>
                            <Type><xsl:value-of select="c:Type"/></Type>
                        </Candy>
                    </xsl:for-each>
                </HighEnergy>
            </EnergyGroups>
        </CategorizedCandies>
    </xsl:template>
</xsl:stylesheet>