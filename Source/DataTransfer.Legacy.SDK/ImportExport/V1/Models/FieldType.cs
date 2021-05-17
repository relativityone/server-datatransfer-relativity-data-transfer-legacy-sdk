namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public enum FieldType
	{
		Empty = -1, // 0xFFFFFFFF
		Varchar = 0,
		Integer = 1,
		Date = 2,
		Boolean = 3,
		Text = 4,
		Code = 5,
		Decimal = 6,
		Currency = 7,
		MultiCode = 8,
		File = 9,
		Object = 10, // 0x0000000A
		User = 11, // 0x0000000B
		LayoutText = 12, // 0x0000000C
		Objects = 13, // 0x0000000D
		OffTableText = 14, // 0x0000000E
	}
}