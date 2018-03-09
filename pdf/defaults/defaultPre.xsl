<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
   <xsl:output omit-xml-declaration="yes" indent="yes" encoding="UTF-8"/>
   <xsl:template match="data">
      <NS1:form xmlns:NS1="http://www.greco.eu/schemas/2014/GOSPOS">
         <xsl:for-each select="*">
            <NS1:item NS1:name="{name()}" NS1:value="{text()}"/>
         </xsl:for-each>
      </NS1:form>
   </xsl:template>
</xsl:stylesheet>