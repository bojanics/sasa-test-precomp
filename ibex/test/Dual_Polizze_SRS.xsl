<?xml version="1.0" encoding="UTF-8"?>

<!-- File was generated by XSLfast 6.0, build 11/09/2012 -->
<!-- Layout version: 7 -->
<!-- Please leave unchanged; manage layouts instead -->
<!DOCTYPE xsl:stylesheet [
<!ENTITY XML "http://www.w3.org/TR/REC-xml">
<!ENTITY XMLNames "http://www.w3.org/TR/REC-xml-names">
<!ENTITY XSLT.ns "http://www.w3.org/1999/XSL/Transform">
<!ENTITY XSLTA.ns "http://www.w3.org/1999/XSL/TransformAlias">
<!ENTITY XSLFO.ns "http://www.w3.org/1999/XSL/Format">
<!ENTITY copy "&#169;">
<!ENTITY trade "&#8482;">
<!ENTITY deg "&#x00b0;">
<!ENTITY gt "&#62;">
<!ENTITY sup2 "&#x00b2;">
<!ENTITY frac14 "&#x00bc;">
<!ENTITY quot "&#34;">
<!ENTITY frac12 "&#x00bd;">
<!ENTITY euro "&#x20ac;">
<!ENTITY Omega "&#937;">
]>


<xsl:stylesheet 
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:func="http://exslt.org/functions"
	xmlns:exslt="http://exslt.org/common"
	xmlns:fox="http://xmlgraphics.apache.org/fop/extensions"
	xmlns:saxon="http://saxon.sf.net/" extension-element-prefixes="saxon"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xslfast="http://www.xslfast.com"
	xmlns:common="http://exslt.org/common"
	xmlns:fo="http://www.w3.org/1999/XSL/Format" version="2.0"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	xmlns:NS1="http://www.greco.eu/schemas/2014/GOSPOS">


<xsl:variable name="path" select="document-uri(document(''))" />
<xsl:variable name="path1" select="base-uri()" />
<xsl:variable name="path2" select="static-base-uri()" />
<xsl:variable name="path3" select="replace(base-uri(),'(.*/)[^/]+?.xml','$1')" />

<xsl:variable name="xxslfile" select="codepoints-to-string(reverse(string-to-codepoints(substring-before(codepoints-to-string(reverse(string-to-codepoints($path))), '/'))))"/> <!-- file alleine -->
<xsl:variable name="xxbasedir" select="substring-before($path2,$xxslfile)" /> <!-- Verzeichnis, von dem aus das xsl file geladen wurde -->


<xsl:variable name="common_img" select="concat($xxbasedir,'IMG','/')" />
<xsl:variable name="special_img" select="concat($xxbasedir,'..','/','IMG','/')" />

<xsl:template match="NS1:form">
<fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
<fo:layout-master-set>
<fo:simple-page-master master-name="masterNamePageMain0" page-height="845.0pt" page-width="598.0pt" margin-top="0.0pt" margin-left="0.0pt" margin-bottom="0.0pt" margin-right="0.0pt" reference-orientation="0" fox:bleed="0pt" fox:crop-box="media-box" fox:crop-offset="0pt" fox:scale="1.0">
<xsl:variable name="backgroundImageRepeat">no-repeat</xsl:variable><fo:region-body margin-left="56.91pt" margin-top="71.13pt" margin-bottom="71.13pt" margin-right="56.91pt"  background-position-horizontal="left" background-position-vertical="top" />
<fo:region-before extent="71.13pt" precedence="true"  background-position-horizontal="left" background-position-vertical="top" />
<fo:region-after extent="71.13pt" precedence="true"  background-position-horizontal="left" background-position-vertical="top" />
<fo:region-start extent="56.91pt"  background-position-horizontal="left" background-position-vertical="top" />
<fo:region-end extent="56.91pt"  background-position-horizontal="left" background-position-vertical="top" />
</fo:simple-page-master>
<fo:simple-page-master master-name="masterNamePageFirst1" page-height="845.0pt" page-width="598.0pt" margin-top="0.0pt" margin-left="0.0pt" margin-bottom="0.0pt" margin-right="0.0pt" reference-orientation="0" fox:bleed="0pt" fox:crop-box="media-box" fox:crop-offset="0pt" fox:scale="1.0">
<xsl:variable name="backgroundImageRepeat">no-repeat</xsl:variable><fo:region-body margin-left="56.91pt" margin-top="71.13pt" margin-bottom="71.13pt" margin-right="56.91pt"  background-position-horizontal="center" background-position-vertical="center" />
<fo:region-before region-name="regionBefore1" extent="71.13pt" precedence="true"  background-position-horizontal="center" background-position-vertical="center" />
<fo:region-after region-name="regionAfter1" extent="71.13pt" precedence="true"  background-position-horizontal="center" background-position-vertical="center" />
<fo:region-start region-name="regionStart1" extent="56.91pt"  background-position-horizontal="center" background-position-vertical="center" />
<fo:region-end region-name="regionEnd1" extent="56.91pt"  background-position-horizontal="center" background-position-vertical="center" />
</fo:simple-page-master>
<fo:simple-page-master master-name="masterNamePageRest2" page-height="845.0pt" page-width="598.0pt" margin-top="0.0pt" margin-left="0.0pt" margin-bottom="0.0pt" margin-right="0.0pt" reference-orientation="0" fox:bleed="0pt" fox:crop-box="media-box" fox:crop-offset="0pt" fox:scale="1.0">
<xsl:variable name="backgroundImageRepeat">no-repeat</xsl:variable><fo:region-body margin-left="56.91pt" margin-top="71.13pt" margin-bottom="71.13pt" margin-right="56.91pt"  background-position-horizontal="left" background-position-vertical="top" />
<fo:region-before region-name="regionBefore2" extent="71.13pt" precedence="true"  background-position-horizontal="left" background-position-vertical="top" />
<fo:region-after region-name="regionAfter2" extent="71.13pt" precedence="true"  background-position-horizontal="left" background-position-vertical="top" />
<fo:region-start region-name="regionStart2" extent="56.91pt"  background-position-horizontal="left" background-position-vertical="top" />
<fo:region-end region-name="regionEnd2" extent="56.91pt"  background-position-horizontal="left" background-position-vertical="top" />
</fo:simple-page-master>
<fo:page-sequence-master master-name="masterSequenceName1">
<fo:repeatable-page-master-alternatives>
<fo:conditional-page-master-reference master-reference="masterNamePageFirst1" page-position="first"/>
<fo:conditional-page-master-reference master-reference="masterNamePageRest2" page-position="rest"/>
</fo:repeatable-page-master-alternatives>
</fo:page-sequence-master>
</fo:layout-master-set>
<fo:bookmark-tree> </fo:bookmark-tree>
<fo:page-sequence master-reference="masterSequenceName1" format="1">
<xsl:attribute name="force-page-count">no-force</xsl:attribute>
<fo:static-content flow-name="regionBefore1">
  <fo:block/>
</fo:static-content>
<fo:static-content flow-name="regionAfter1">
<fo:block-container display-align="before" reference-orientation="0">
<fo:block position="relative" text-align="start" color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="11.0pt" white-space-collapse="false"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  hyphenate="true" language="en"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" width="100.0pt"  height="30.0pt" ><fo:block absolute-position="absolute" top="25cm" left="0cm">
	    <fo:external-graphic src="url({concat($common_img,'dual_policy_footer.jpg')})" content-width="21.12cm" />
</fo:block>
</fo:block>
</fo:block-container>
</fo:static-content>
<fo:static-content flow-name="regionStart1">
  <fo:block/>
</fo:static-content>
<fo:static-content flow-name="regionEnd1">
  <fo:block/>
</fo:static-content>
<fo:static-content flow-name="regionBefore2">
<fo:block-container position="absolute" top="24.0pt" left="40.0pt" height="22.0pt" width="382.0pt" display-align="after" reference-orientation="0" fox:transform="rotate(0) , translate(0, 0)">
<fo:block text-align="start" color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="8.0pt" padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" position="relative" height="22.0pt" width="382.0pt" keep-together="auto"  white-space-collapse="false"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  hyphenate="true" language="en" ><xsl:text>Seite </xsl:text>
<fo:page-number/>
<xsl:text> zur Strafrechtsschutzversicherung Nr. </xsl:text>
<xsl:value-of select="NS1:item[@NS1:name='policy_no_srs']/@NS1:value" />
</fo:block>
</fo:block-container>
</fo:static-content>
<fo:static-content flow-name="regionAfter2">
<fo:block-container display-align="before" reference-orientation="0">
<fo:block position="relative" text-align="start" color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="11.0pt" white-space-collapse="false"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  hyphenate="true" language="en"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" width="100.0pt"  height="30.0pt" ><fo:block absolute-position="absolute" top="25cm" left="0cm">
	    <fo:external-graphic src="url({concat($common_img,'dual_policy_footer.jpg')})" content-width="21.12cm" />
</fo:block>
</fo:block>
</fo:block-container>
</fo:static-content>
<fo:static-content flow-name="regionStart2">
  <fo:block/>
</fo:static-content>
<fo:static-content flow-name="regionEnd2">
  <fo:block/>
</fo:static-content>
<fo:flow flow-name="xsl-region-body">
<fo:block/>
<fo:block-container display-align="before" reference-orientation="0">
<fo:block position="relative" text-align="center" color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="11.0pt" line-height="14.5pt" white-space-collapse="false"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  hyphenate="true" language="en"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" width="471.0pt"  height="30.0pt" ><xsl:call-template name="policy_SRS" />

<fo:block space-before="1cm" text-align="center">
	<xsl:call-template name="erstellt" />
</fo:block>

<fo:block space-before="2cm" >
	<xsl:call-template name="signatures" />
</fo:block></fo:block>
</fo:block-container>
<fo:block-container display-align="before" reference-orientation="0">
<fo:block position="relative" text-align="start" color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="11.0pt" line-height="14.5pt" white-space-collapse="false"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  hyphenate="true" language="en"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" width="100.0pt"  height="30.0pt" ><fo:block page-break-before="always">
	<xsl:call-template name="versicherer" />
</fo:block></fo:block>
</fo:block-container>
<fo:block-container display-align="before" reference-orientation="0">
<fo:block position="relative" text-align="start" color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="11.0pt" line-height="14.5pt" white-space-collapse="false"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  hyphenate="true" language="en"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" width="100.0pt"  height="30.0pt" ><fo:block page-break-before="always">
	<xsl:call-template name="Anhang" />
</fo:block></fo:block>
</fo:block-container>
<xsl:if test='position()=last()'>
  <fo:block id="lastPage"/>
</xsl:if>
</fo:flow>
</fo:page-sequence>
</fo:root>
</xsl:template>

<!-- GENERATED TEMPLATE Anhang -->
<xsl:template name="Anhang" >
<xsl:variable name="backgroundImageTextFlowVariable0"></xsl:variable>
<fo:block-container display-align="before" reference-orientation="0" font-stretch="normal" text-transform="none" fox:transform="rotate(0) , translate(0, 0)">
<fo:block linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" line-height="14.5pt" white-space-collapse="false"  hyphenate="true" language="en"  text-align="start" position="relative" height="30.0pt" width="179.0pt" keep-together="auto"  color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Arial" font-size="12.0pt" letter-spacing="normal" word-spacing="normal" font-stretch="normal" text-transform="none">
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Arial" font-size="12.0pt">
<xsl:text>Anhang...</xsl:text></fo:inline>
</fo:block>
</fo:block-container>
</xsl:template>

<!-- GENERATED TEMPLATE policy_SRS -->
<xsl:template name="policy_SRS" >
<fo:block-container display-align="before" reference-orientation="0">
<fo:block position="relative" text-align="start" color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="11.0pt" white-space-collapse="false"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  hyphenate="true" language="en"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" width="436.0pt"  height="30.0pt" ><fo:block-container>
	<fo:block position="relative" font-family="Arial" font-size="10pt" text-align="left" >
		<fo:table text-align="left" vertical-align="center" padding-top="0.2cm" padding-bottom="0.2cm"  table-omit-header-at-break="true">
			<fo:table-column column-width="30%" />
			<fo:table-column column-width="20%" />
			<fo:table-column column-width="20%" />
			<fo:table-column column-width="20%" />


<fo:table-header border-style="none" >                            
      <fo:table-row >                         
        <fo:table-cell number-columns-spanned="4">                      
	<fo:block text-align="center" space-after="10pt">
		<fo:inline font-family="Arial" font-size="16pt" font-style="bold">DUAL Polizze</fo:inline>
	</fo:block>		
	<fo:block text-align="center">	
		<xsl:text>zur </xsl:text>
	</fo:block>		
	<fo:block  text-align="center" space-after="10pt">
		<fo:inline font-family="Arial" font-size="14pt" font-style="bold">Strafrechtsschutzversicherung</fo:inline>
	</fo:block>			
        </fo:table-cell>
     </fo:table-row>
</fo:table-header>
			
			
			
			<fo:table-body>
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Versicherungsscheinnummer:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell number-columns-spanned="3" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:value-of select="NS1:item[@NS1:name='policy_no_srs']/@NS1:value" />
						</fo:block>
					</fo:table-cell>
				</fo:table-row>
		

<!-- VersicherungsnehmerIn: -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>VersicherungsnehmerIn:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block>
								<xsl:value-of select="NS1:item[@NS1:name='name_client']/@NS1:value" />
							<fo:block>
								<xsl:value-of select="NS1:item[@NS1:name='street_client']/@NS1:value" />
							</fo:block>
							<fo:block>
								<xsl:value-of select="concat(NS1:item[@NS1:name='zip_client']/@NS1:value, ' ', NS1:item[@NS1:name='city_client']/@NS1:value)" />
							</fo:block>
							<fo:block>
								<xsl:text>FEHTLT LAND!!!</xsl:text>
							</fo:block>
						</fo:block>
					</fo:table-cell>
				</fo:table-row>

			

<!-- Assekuradeur -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Assekuradeur:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block>
								<xsl:text>DUAL Deutschland GmbH</xsl:text>
							<fo:block>
								<xsl:text>Schanzenstraße 36 / Gebäude 197</xsl:text>
							</fo:block>
							<fo:block>
								<xsl:text>51063 Köln</xsl:text>
							</fo:block>
							<fo:block>
								<xsl:text>für den vertretenen Versicherer</xsl:text>
							</fo:block>
						</fo:block>
					</fo:table-cell>
				</fo:table-row>

<!-- Versicherungsmakler -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm">
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Versicherungsmakler:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<xsl:if test="NS1:item[@NS1:name='branding']/@NS1:value='GrECo'">
<fo:block text-align="left">
	<fo:inline font-family="Arial" font-size="10pt" color="black">
		<xsl:text>GrECo International AG</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>Elmargasse 2-4</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>1191 Wien</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>ÖSTERREICH</xsl:text>
	</fo:inline>
</fo:block>
</xsl:if>

<xsl:if test="NS1:item[@NS1:name='branding']/@NS1:value='VMG'">
<fo:block text-align="left">
	<fo:inline font-family="Arial" font-size="10pt" color="black">
		<xsl:text>VMG Versicherungsmakler GmbH</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>Berggasse 31</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>1090 Wien</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>ÖSTERREICH</xsl:text>
	</fo:inline>
</fo:block>
</xsl:if>

<xsl:if test="NS1:item[@NS1:name='branding']/@NS1:value='BA-CA'">
<fo:block text-align="left">
	<fo:inline font-family="Arial" font-size="10pt" color="black">
		<xsl:text>BA~CA GrECo Versicherungsmanagement GmbH</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>Elmargasse 2-4</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>1191 Wien</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>ÖSTERREICH</xsl:text>
	</fo:inline>
</fo:block>
</xsl:if>

<xsl:if test="NS1:item[@NS1:name='branding']/@NS1:value='EGHV'">
<fo:block text-align="center">
	<fo:inline font-family="Arial" font-size="10pt" color="black">
		<xsl:text>Ecclesia GrECo Hospital Versicherungsmakler GmbH</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>Elmargasse 2-4</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>1191 Wien</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>ÖSTERREICH</xsl:text>
	</fo:inline>
</fo:block>
</xsl:if>

<xsl:if test="NS1:item[@NS1:name='branding']/@NS1:value='GJRC'">
<fo:block text-align="center">
	<fo:inline font-family="Arial" font-size="10pt" color="black">
		<xsl:text>GrECo JLT Risk Consulting GmbH</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>Elmargasse 2-4</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>1191 Wien</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>ÖSTERREICH</xsl:text>
	</fo:inline>
</fo:block>
</xsl:if>

<xsl:if test="NS1:item[@NS1:name='branding']/@NS1:value='CMV'">
<fo:block text-align="center">
	<fo:inline font-family="Arial" font-size="10pt" color="black">
		<xsl:text>CMV - Christian Mädel Versicherungsmakler GmbH</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>Hungerbergstraße 1A/5</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>1190 Wien</xsl:text>
		<xsl:text>&#x0a;</xsl:text>
		<xsl:text>ÖSTERREICH</xsl:text>
	</fo:inline>
</fo:block>
</xsl:if>
					</fo:table-cell>
				</fo:table-row>



<!-- Versicherungsdauer: -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm">
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Versicherungsdauer:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" >
							<fo:block>
								<xsl:text>Versicherungsbeginn:</xsl:text>
							</fo:block>
							<fo:block>
								<xsl:text>Versicherungsablauf:</xsl:text>
							</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" >
							<fo:block>
								<xsl:value-of select="NS1:item[@NS1:name='begin_date_srs']/@NS1:value" />
							</fo:block>
							<fo:block>
								<xsl:value-of select="NS1:item[@NS1:name='end_date_srs']/@NS1:value" />
							</fo:block>
					</fo:table-cell>					
				</fo:table-row>


<!-- Fälligkeit: -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Nächste Fälligkeit:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block>
								<xsl:value-of select="NS1:item[@NS1:name='due_date_srs']/@NS1:value" />
							</fo:block>
					</fo:table-cell>
				</fo:table-row>


<!-- Versicherungssumme: -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Versicherungssumme:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block>
								<xsl:value-of select="concat('EUR ',translate(format-number(number(NS1:item[@NS1:name='sum_insured_srs']/@NS1:value),'#,##0.00'),'.,',',.'))" />
							</fo:block>
					</fo:table-cell>
				</fo:table-row>


<!-- Jahresnettoprämie: -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Jahresnettoprämie:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block>
								<xsl:value-of select="concat('EUR ',translate(format-number(number(NS1:item[@NS1:name='premium_srs']/@NS1:value),'#,##0.00'),'.,',',.'))" />
							</fo:block>
					</fo:table-cell>
				</fo:table-row>


<!-- Versicherungssteuer: -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>+ 11,00% Versicherungssteuer:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block>
								<xsl:value-of select="concat('EUR ',translate(format-number(number(NS1:item[@NS1:name='premium_srs']/@NS1:value)*0.11,'#,##0.00'),'.,',',.'))" />
							</fo:block>
					</fo:table-cell>
				</fo:table-row>


<!-- Jahresbruttoprämie: -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Jahresbruttoprämie:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block>
								<xsl:value-of select="concat('EUR ',translate(format-number(number(NS1:item[@NS1:name='premium_srs']/@NS1:value)*1.11,'#,##0.00'),'.,',',.'))" />
							</fo:block>
					</fo:table-cell>
				</fo:table-row>


<!-- Recht: -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Anzuwendendes Recht:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block>
								<xsl:text>Für Streitigkeiten aus diesem Vertrag gilt ausschließlich österreichisches Recht.</xsl:text>
							</fo:block>
					</fo:table-cell>
				</fo:table-row>


<!-- Bedingungen -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block>
							<xsl:text>Versicherungsbedingungen:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block>
								<xsl:text>Dual AVBST 2018 GrECo (2018/04)</xsl:text>
							</fo:block>
					</fo:table-cell>
				</fo:table-row>



<!-- Schaden -->
				<fo:table-row padding-top="0.2cm" padding-bottom="0.2cm" >
					<fo:table-cell  font-weight="bold" padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm">
						<fo:block page-break-before="always" >
							<xsl:text>Schadenmeldung:</xsl:text>
						</fo:block>
					</fo:table-cell>
					<fo:table-cell padding-top="0.2cm" padding-bottom="0.2cm" padding-left="0.2cm" number-columns-spanned="3">
							<fo:block space-after="0.5cm">
								<xsl:text>Im Schadenfall wenden Sie sich bitte an:</xsl:text>
							</fo:block>
							<fo:block>
								<xsl:text>DUAL Deutschland GmbH</xsl:text>
							<fo:block>
								<xsl:text>Schanzenstraße 36 / Gebäude 197</xsl:text>
							</fo:block>
							<fo:block>
								<xsl:text>51063 Köln</xsl:text>
							</fo:block>
						</fo:block>
					</fo:table-cell>
				</fo:table-row>

			</fo:table-body>
		</fo:table>
</fo:block>		
</fo:block-container></fo:block>
</fo:block-container>
</xsl:template>

<!-- GENERATED TEMPLATE erstellt -->
<xsl:template name="erstellt" >
<fo:block break-before="auto"/>
<fo:block-container height="45.0pt" width="540.6pt" font-stretch="normal" text-transform="none">
<fo:block linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" line-height="11.38pt" white-space-collapse="false"  hyphenate="true" language="en"  text-align="center" width="540.6pt"  height="45.0pt"  keep-together="auto"  letter-spacing="normal" word-spacing="normal" font-stretch="normal" text-transform="none">
<fo:block text-align="center" white-space-collapse="false"  hyphenate="true" language="en"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed" >
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="12.0pt" font-weight="bold">
<xsl:text>Erstellt im Auftrag und mit Vollmacht des Versicherers.</xsl:text></fo:inline>
</fo:block>
<fo:block text-align="center" white-space-collapse="false"  hyphenate="true" language="en"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed" >
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="12.0pt" font-weight="bold">
</fo:inline>
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="12.0pt" font-weight="bold">
<xsl:text>DUAL Deutschland GmbH</xsl:text></fo:inline>
</fo:block>
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="10.0pt">
<xsl:text>Schanzenstraße 36 / Gebäude 197 51063 Köln</xsl:text></fo:inline>
<fo:block text-align="center" white-space-collapse="false"  hyphenate="true" language="en"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed" >
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="10.0pt">
</fo:inline>
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="10.0pt">
<xsl:text>Telefon: +49 221 168026-0 Telefax: +49 221 168026-66</xsl:text></fo:inline>
</fo:block>
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="10.0pt">
<xsl:text>info@dualdeutschland.com</xsl:text></fo:inline>
<fo:block text-align="center" white-space-collapse="false"  hyphenate="true" language="en"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed" >
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="10.0pt">
</fo:inline>
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="10.0pt">
<xsl:text>Eingetragen beim Amtsgericht Köln - HRB 56034</xsl:text></fo:inline>
</fo:block>
<fo:block text-align="center" white-space-collapse="false"  hyphenate="true" language="en"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed" >
<fo:inline color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="10.0pt">
<xsl:text>
</xsl:text></fo:inline>
</fo:block>
</fo:block>
</fo:block-container>
<fo:block break-after="auto"/>
</xsl:template>

<!-- GENERATED TEMPLATE signatures -->
<xsl:template name="signatures" >
<fo:block-container display-align="before" reference-orientation="0">
<fo:block position="relative" text-align="start" color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="11.0pt" line-height="14.5pt" white-space-collapse="false"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  hyphenate="true" language="en"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" width="411.0pt"  height="30.0pt" ><fo:block>
<fo:table text-align="center" display-align="center">
<fo:table-column column-number="1" column-width="33.33%" />
<fo:table-column column-number="2" column-width="33.33%" />
<fo:table-column column-number="3" column-width="33.33%" />
   <fo:table-body>                                              
      <fo:table-row>                                           
         <fo:table-cell number-rows-spanned="2"> 
            <fo:block><xsl:value-of select="concat('Köln, ',format-date(current-date(),'[D01].[M01].[Y0001]'))"/></fo:block>                         
         </fo:table-cell>                         
         <fo:table-cell> 
            <fo:block>
	    	<fo:external-graphic src="url({concat($common_img,'signature_1.jpg')})" content-width="5cm" />
	    </fo:block>
         </fo:table-cell>
         <fo:table-cell> 
            <fo:block>
	    	<fo:external-graphic src="url({concat($common_img,'signature_2.jpg')})" content-width="5cm" />
	    </fo:block>
         </fo:table-cell>                         
      </fo:table-row>
      
      <fo:table-row>                                           
         <fo:table-cell padding-top="-1.5cm"> 
            <fo:block>ppa. Michael Moersch</fo:block>
         </fo:table-cell>
         <fo:table-cell padding-top="-1.5cm"> 
            <fo:block>ppa. Kathrin Probst</fo:block>
         </fo:table-cell>
      </fo:table-row>
      
   </fo:table-body>
</fo:table>
</fo:block></fo:block>
</fo:block-container>
</xsl:template>

<!-- GENERATED TEMPLATE versicherer -->
<xsl:template name="versicherer" >
<fo:block-container display-align="before" reference-orientation="0">
<fo:block position="relative" text-align="start" color="rgb-icc(0,0,0, #CMYK, 0.65,0.53,0.51,1.0)" font-family="Calibri" font-size="11.0pt" line-height="14.5pt" white-space-collapse="false"  linefeed-treatment="preserve" white-space-treatment="ignore-if-surrounding-linefeed"  hyphenate="true" language="en"  padding-bottom="0.0pt" start-indent="0.0pt" end-indent="0.0pt" padding-top="0.0pt" padding="0.0pt" width="415.0pt"  height="30.0pt" ><fo:block>
<fo:table>
<fo:table-column column-number="1" column-width="80%" />
<fo:table-column column-number="2" column-width="20%" />
   <fo:table-body>                                              
      <fo:table-row>                                           
         <fo:table-cell number-columns-spanned="2"> 
            <fo:block font-style="bold">Versicherer</fo:block>                         
         </fo:table-cell>                         
      </fo:table-row>
      
      <fo:table-row>                                           
         <fo:table-cell number-columns-spanned="2"> 
            <fo:block><xsl:text>&#x0a;</xsl:text></fo:block>
         </fo:table-cell>
      </fo:table-row>

      <fo:table-row>                                           
         <fo:table-cell> 
            <fo:block>Versicherer dieses Vertrags ist:</fo:block>
         </fo:table-cell>
         <fo:table-cell> 
            <fo:block text-align="right">Beteiligung</fo:block>
         </fo:table-cell>
      </fo:table-row>

      <fo:table-row>                                           
         <fo:table-cell number-columns-spanned="2"> 
            <fo:block><xsl:text>&#x0a;</xsl:text></fo:block>
         </fo:table-cell>
      </fo:table-row>


      <fo:table-row>                                           
         <fo:table-cell> 
            <fo:block>Liberty Mutual Insurance Europe Ltd.</fo:block>
         </fo:table-cell>
         <fo:table-cell> 
            <fo:block text-align="right">100,00 %</fo:block>
         </fo:table-cell>
      </fo:table-row>

      <fo:table-row>                                           
         <fo:table-cell number-columns-spanned="2"> 
            <fo:block>23rd Floor, 20 Femchurch Street</fo:block>
         </fo:table-cell>
      </fo:table-row>

      <fo:table-row>                                           
         <fo:table-cell number-columns-spanned="2"> 
            <fo:block>EC3M 3AW London</fo:block>
         </fo:table-cell>
      </fo:table-row>

      <fo:table-row>                                           
         <fo:table-cell number-columns-spanned="2"> 
            <fo:block text-align="start">
        	<fo:leader leader-pattern="rule"
                   rule-thickness="0.1pt"
                   leader-length="17cm"
                   start-indent="0cm"
                   end-indent="0cm"
                   color="black"/>
	    </fo:block>
         </fo:table-cell>
      </fo:table-row>
     
   </fo:table-body>
</fo:table>
</fo:block></fo:block>
</fo:block-container>
</xsl:template>
<xsl:template match="include-xsl-fo">
    <xsl:copy-of select="@*"/>
</xsl:template>
</xsl:stylesheet>
