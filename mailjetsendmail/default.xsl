<xsl:stylesheet xmlns:fox="http://xml.apache.org/fop/extensions" xmlns:NS1="http://www.greco.eu/schemas/2014/GOSPOS" xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:saxon="http://icl.com/saxon" version="1.1" extension-element-prefixes="saxon">
<xsl:template match="*">
 <xsl:text>Dear Sir,</xsl:text>
 <xsl:text></xsl:text>
 <xsl:text>I'm MailJet function.</xsl:text>
 <xsl:text></xsl:text>
 <xsl:text></xsl:text>
 <xsl:text>HERE IS THE DATA:</xsl:text>
 <xsl:text></xsl:text>
 <xsl:for-each select="NS1:item">
  <xsl:if test="@NS1:name">
   <xsl:value-of select="concat(@NS1:name,': ')"/>
   <xsl:value-of select="@NS1:value"/>
   <xsl:text></xsl:text>
  </xsl:if>
 </xsl:for-each>
 <xsl:for-each select="property">
  <xsl:value-of select="concat(@name,': ')"/>
   <xsl:value-of select="@value"/>
   <xsl:text></xsl:text>
 </xsl:for-each>
 <xsl:text></xsl:text>
 <xsl:text>Best Regards,</xsl:text>
 <xsl:text></xsl:text>
 <xsl:text>MailJet</xsl:text>
</xsl:template>
</xsl:stylesheet>