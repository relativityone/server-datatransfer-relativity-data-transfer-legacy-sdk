CAST(N'<auditElement>' AS NVARCHAR(MAX)) +
		'<field id="100123" type="8" name="MultiCode Field" formatstring="">' + 
			ISNULL(CAST(GM.[MultiCodeField IsNew] AS NVARCHAR(MAX)) COLLATE Test_Collation, '') +
			ISNULL(CAST(GM.[MultiCodeField] AS NVARCHAR(MAX)) COLLATE Test_Collation, '') +
		'</field>' +
	'</auditElement>',