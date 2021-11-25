CAST(N'<auditElement>' AS NVARCHAR(MAX)) +
	'<extractedTextEncodingPageCode>' + ISNULL(CAST(N.[ExtractedTextEncodingPageCode] AS NVARCHAR(200)), '-1') + '</extractedTextEncodingPageCode>' +
	'</auditElement>',