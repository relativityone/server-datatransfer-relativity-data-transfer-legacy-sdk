CAST(N'<auditElement>' AS NVARCHAR(MAX)) +
		'<field id="100123" type="3" name="Boolean Field" formatstring="">' + 
			'<oldValue>' + CASE  DELETED.[BooleanField] WHEN 1 THEN 'True' WHEN 0 THEN 'False' ELSE '' END + '</oldValue>' +
			'<newValue>' + CASE INSERTED.[BooleanField] WHEN 1 THEN 'True' WHEN 0 THEN 'False' ELSE '' END + '</newValue>' +
		'</field>' +
		'<field id="100123" type="5" name="Code Field" formatstring="">' + 
			ISNULL('<setChoice>' + NULLIF(N.[CodeField] COLLATE Test_Collation, '') + '</setChoice>', '') +
			ISNULL(GM.[CodeField] COLLATE Test_Collation, '') +
		'</field>' +
		'<field id="100123" type="2" name="Date Field" formatstring="">' + 
			'<oldValue>' + ISNULL(CONVERT(NVARCHAR(23),  DELETED.[DateField], 120) COLLATE Test_Collation, '') + '</oldValue>' +
			'<newValue>' + ISNULL(CONVERT(NVARCHAR(23), INSERTED.[DateField], 120) COLLATE Test_Collation, '') + '</newValue>' +
		'</field>' +
		'<field id="100123" type="9" name="File Field" formatstring="">' + 
			'<oldValue>' + ISNULL(CONVERT(NVARCHAR(200), F.[FileName]) COLLATE Test_Collation, '') + '</oldValue>' +
			'<newValue>' + ISNULL(CONVERT(NVARCHAR(200), N.[FileField_ImportObject_FileName]) COLLATE Test_Collation, '') + '</newValue>' +
		'</field>' +
		'<field id="100123" type="10" name="Object Field" formatstring="">' + 
			'<oldValue>' + ISNULL(CONVERT(NVARCHAR(MAX),  DELETED.[ObjectField]) COLLATE Test_Collation, '') + '</oldValue>' +
			'<newValue>' + ISNULL(CONVERT(NVARCHAR(MAX), INSERTED.[ObjectField]) COLLATE Test_Collation, '') + '</newValue>' +
		'</field>' +
		'<field id="100123" type="4" name="Text Field" formatstring="">' + 
			'<oldValue>' + IIF(DATALENGTH(DELETED.[TextField]) > 1000000, '<![CDATA[NOTE:Contents of field are longer than 1MB; value-audit skipped]]>', '<![CDATA[' + ISNULL(DELETED.[TextField] COLLATE Test_Collation, '') + ']]>') + '</oldValue>' +
			'<newValue>' + IIF(DATALENGTH(INSERTED.[TextField]) > 1000000, '<![CDATA[NOTE:Contents of field are longer than 1MB; value-audit skipped]]>', '<![CDATA[' + ISNULL(INSERTED.[TextField] COLLATE Test_Collation, '') + ']]>') + '</newValue>' +
		'</field>' +
		'<field id="100123" type="0" name="Varchar Field" formatstring="">' + 
			'<oldValue><![CDATA[' + ISNULL(DELETED.[VarcharField] COLLATE Test_Collation, '') + ']]></oldValue>' +
			'<newValue><![CDATA[' + ISNULL(INSERTED.[VarcharField] COLLATE Test_Collation, '') + ']]></newValue>' +
		'</field>' +
	'</auditElement>',