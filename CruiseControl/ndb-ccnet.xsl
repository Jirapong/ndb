<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="html" />

  <xsl:template match="//ndb">
    <strong>ndb Summary</strong>
    <style type="text/css">
        .text { pending: 0px; margin: 2px; font-size: small;}
      </style>
    <br/>

    <xsl:for-each select="message">
      <p class="text"><xsl:value-of select="text"/></p>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>
