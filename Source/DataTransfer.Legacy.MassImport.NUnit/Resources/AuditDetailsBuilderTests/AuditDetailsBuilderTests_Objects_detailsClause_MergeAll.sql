	CAST(N'<auditElement>' AS NVARCHAR(MAX)) +
'
<field id="100123" type="13" name="Objects Field" formatstring="">' +
			ISNULL(CAST(GM.[ObjectsField IsNew] AS NVARCHAR(MAX)) COLLATE Test_Collation, '')  +
'</field>' +
	'</auditElement>',