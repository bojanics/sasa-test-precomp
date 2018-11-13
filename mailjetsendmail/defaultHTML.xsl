<xsl:stylesheet xmlns:fox="http://xml.apache.org/fop/extensions" xmlns:NS1="http://www.greco.eu/schemas/2014/GOSPOS" xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:saxon="http://icl.com/saxon" version="1.1" extension-element-prefixes="saxon">
<xsl:template match="*">
 <html>
 <body>
 <h2>Dear Sir,</h2>
 <xsl:text></xsl:text>
 <xsl:text>I'm MailJet function.</xsl:text>
 <xsl:text></xsl:text>
 <xsl:text></xsl:text>
 <h4>HERE IS THE DATA:</h4>
 <xsl:text></xsl:text>
 <table border="2">
  <tr>
   <th>Name</th>
   <th>Value</th>
  </tr>
 <xsl:for-each select="NS1:item">
  <xsl:if test="@NS1:name">
   <tr>
    <td>
     <xsl:value-of select="@NS1:name"/>
    </td>
    <td>
     <xsl:value-of select="@NS1:value"/>
    </td>
   </tr>
  </xsl:if>
 </xsl:for-each>
 <xsl:for-each select="property">
  <tr>
   <td>
    <xsl:value-of select="@name"/>
   </td>
   <td>
    <xsl:value-of select="@value"/>
   </td>
  </tr>
 </xsl:for-each>
 </table>
 <xsl:text></xsl:text>
 <h4>Best Regards,</h4>
 <xsl:text></xsl:text>
 <h3>MailJet</h3>
 </body>
 </html>
</xsl:template>
</xsl:stylesheet>