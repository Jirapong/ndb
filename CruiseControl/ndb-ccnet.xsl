<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="html" />

  <xsl:template match="/">
    <div id="ndb-report">
      <style type="text/css">
        .text { pending: 0px; margin: 2px; font-size: small;}
      </style>
      <xsl:apply-templates select="//ndb"/>
    </div>
  </xsl:template>

  <xsl:template match="//ndb">
    <h1>ndb Summary</h1>
    <br/>

    <xsl:for-each select="message">
      <p class="text"><xsl:value-of select="text"/></p>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>
