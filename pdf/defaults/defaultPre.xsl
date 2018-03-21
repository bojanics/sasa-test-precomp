<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
   <xsl:output omit-xml-declaration="yes" indent="yes" encoding="UTF-8"/>
   <xsl:template match="data">
      <NS1:form xmlns:NS1="http://www.greco.eu/schemas/2014/GOSPOS">
         <xsl:if test="property[@name='submission_at']">
            <xsl:attribute name="NS1:at">
               <xsl:value-of select="property[@name='submission_at']/@value" />
            </xsl:attribute>
         </xsl:if>
         <xsl:if test="property[@name='submission_button']">
            <xsl:attribute name="NS1:button">
               <xsl:value-of select="property[@name='submission_button']/@value" />
            </xsl:attribute>
         </xsl:if>
         <xsl:if test="property[@name='submission_by']">
            <xsl:attribute name="NS1:by">
               <xsl:value-of select="property[@name='submission_by']/@value" />
            </xsl:attribute>
         </xsl:if>
         <xsl:if test="property[@name='submission_gosposversion']">
            <xsl:attribute name="NS1:gosposversion">
               <xsl:value-of select="property[@name='submission_gosposversion']/@value" />
            </xsl:attribute>
         </xsl:if>
         <xsl:if test="property[@name='submission_host']">
            <xsl:attribute name="NS1:host">
               <xsl:value-of select="property[@name='submission_host']/@value" />
            </xsl:attribute>
         </xsl:if>
         <xsl:if test="property[@name='submission_form_number']">
            <xsl:attribute name="NS1:number">
               <xsl:value-of select="property[@name='submission_form_number']/@value" />
            </xsl:attribute>
         </xsl:if>
         <xsl:if test="property[@name='submission_useragent']">
            <xsl:attribute name="NS1:useragent">
               <xsl:value-of select="property[@name='submission_useragent']/@value" />
            </xsl:attribute>
         </xsl:if>
         <xsl:if test="property[@name='submission_form_version']">
            <xsl:attribute name="NS1:version">
               <xsl:value-of select="property[@name='submission_form_version']/@value" />
            </xsl:attribute>
         </xsl:if>
         
         <xsl:for-each select="property">
            <xsl:if test="@name!='submission_form_namepsace' and @name!='submission_at' and @name!='submission_button' and @name!='submission_by' and @name!='submission_gosposversion' and @name!='submission_host' and @name!='submission_form_number' and @name!='submission_useragent' and @name!='submission_form_version'">
               <NS1:item NS1:name="{@name}" NS1:value="{@value}"/>
            </xsl:if>
         </xsl:for-each>
         <xsl:for-each select="array[@name='attachments']">
            <xsl:for-each select="complexobject[@name='attachments']">
               <NS1:attachment>            
                  <xsl:if test="property[@name='name']">
                     <xsl:attribute name="NS1:name">
                        <xsl:value-of select="property[@name='name']/@value" />
                     </xsl:attribute>
                  </xsl:if>
                  <xsl:if test="property[@name='description']">
                     <xsl:attribute name="NS1:description">
                        <xsl:value-of select="property[@name='description']/@value" />
                     </xsl:attribute>
                  </xsl:if>
                  <xsl:if test="property[@name='type']">
                     <xsl:attribute name="NS1:type">
                        <xsl:value-of select="property[@name='type']/@value" />
                     </xsl:attribute>
                  </xsl:if>
                  <xsl:if test="property[@name='content_base64']">
                     <xsl:attribute name="NS1:content">
                        <xsl:value-of select="property[@name='content_base64']/@value" />
                     </xsl:attribute>
                  </xsl:if>
               </NS1:attachment>
            </xsl:for-each> 
         </xsl:for-each> 
         
      </NS1:form>
   </xsl:template>
</xsl:stylesheet>