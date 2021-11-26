
export interface OITStringMap  { [key: string]: string }
export interface OITValueMap   { [key: string]: any }

/*export enum OITRangeType {
    text = "text",
    cell = "cell",
    rect = "rect"
}*/

export interface OITCellRange
	{
	readonly type: string /*OITRangeType*/; /** The range type ("cell") */
	page:     number; /** The 0-based page number containing the range */
	section?: number; /** The section number associated with the page */
	c1:       number; /** The 0-based index of the leftmost column of the range */
	r1:       number; /** The 0-based index of the topmost row of the range */
	c2:       number; /** The 0-based index of the rightmost column of the range */
	r2:       number; /** The 0-based index of the bottommost row of the range */
	}


/*export enum RectRangeUnits {
	px  = "px",
	tw  = "tw",
	pct = "pct"
	}*/

export interface OITRectRange
	{
	readonly type: string /*OITRangeType*/; /** The range type ("rect") */
	page:    number;
	left:    number;
	top:     number;
	right:   number;
	bottom:  number;
	units:   string /*RectRangeUnits*/;
	embedId: number|undefined;
	}


export interface OITTextRange
	{
	readonly type: string /*OITRangeType*/; /** The range type ("text") */
	acc:    number;
	accend: number;
	}



export interface OITDocumentPageInfo
	{
	/* TODO */
	}

export interface OITDocumentFileInfo
	{
	/* TODO */
	}

export interface OITDocumentFontInfo
	{
	/* TODO */
	}

export interface OITDocumentFileMap
	{
	readonly page?:    OITDocumentFileInfo[]; /** Files with the content of a single page */
	readonly data?:    OITDocumentFileInfo[]; /** Files containing data for a single spreadsheet or database table (or a portion thereof) */
	readonly text?:    OITDocumentFileInfo[]; /** Files containing raw text data as UTF-8 */
	readonly content?: OITDocumentFileInfo[]; /** Files containing document structure and metadata information */
	readonly style?:   OITDocumentFileInfo[]; /** CSS files */
	readonly image?:   OITDocumentFileInfo[]; /** Exported embedded images and brush patterns */
	readonly font?:    OITDocumentFileInfo[]; /** Exported fonts */
	readonly aux?:     OITDocumentFileInfo[]; /** Other exported files that aren't in one of the above categories */
	}

export interface OITDocumentManifest
	{
	readonly pages?:           OITDocumentPageInfo[]; /** A list of the pages in the document */
	readonly requestedFonts?:  OITDocumentFontInfo[]; /** A list of the fonts that were requested during the export of the document */
	readonly files?:           OITDocumentFileMap;    /** An object where each property is an array of files of a certain type that were produced during export. */
	}

export interface OITViewInfo
	{
	filename: string; /** The filename of the original input file, if available. */
	fi:       number; /** The integer FI id for the input document. See SCCFI.H in the WebView sdk/common directory for a list of possible FI values. */
	fistring: string; /** The name of the input document's format id */
	product:  string; /** The name of the OIT product that generated the output (typically "Outside In® Technology") */
	version:  string; /** The version number of the OIT product that generated the output */
	libversion: string; /** The version number of the API's Javascript library (oit.js) */
	manifest: OITDocumentManifest;
	}

export interface OIDocumentConfig
	{
	headers?: { [key: string]: string }
	withCredentials?: boolean;
	}

export interface OITView
	{
	attach( containerElem: HTMLElement ): void;
	detach(): void;
	load( url: string, config?: OIDocumentConfig ): void;
	unload(): void;
	fitToContainer(): void;
	width( val?: number ): number;
	height( val?: number ): number;
	info(): OITViewInfo;
	hasFeature( name: string ): boolean;
	}



export interface OITDocumentGeneralProperties
	{
	readonly author?:       string; /** The document author name */
	readonly category?:     string; /** The document category */
	readonly company?:      string; /** The document company */
	readonly creationDate?: string; /** The date the document was created */
	readonly keywords?:     string; /** The document keywords */
	readonly lastSaveDate?: string; /** The date the document was last saved */
	readonly subject?:      string; /** The document subject */
	readonly title?:        string; /** The document title */
	}

export interface OITDocumentStatistics
	{
	readonly backupDate?:          string; /** The date the document was last backed up */
	readonly byteCount?:           string; /** The number of bytes in the document */
	readonly charCount?:           string; /** The number of characters in the document */
	readonly charCountWithSpaces?: string; /** The number of characters in the document, including spaces */
	readonly completedDate?:       string; /** The date the document was completed */
	readonly editMinutes?:         string; /** The total number of minutes spent editing the document */
	readonly hiddenSlideCount?:    string; /** The number of hidden slides in the document */
	readonly lastPrintDate?:       string; /** The date the document was last printed */
	readonly lastSavedBy?:         string; /** The person who last saved the document */
	readonly lineCount?:           string; /** The number of lines in the document */
	readonly noteCount?:           string; /** The number of notes in the document */
	readonly pageCount?:           string; /** The number of pages in the document */
	readonly paraCount?:           string; /** The number of paragraphs in the document */
	readonly revisionDate?:        string; /** The date of the current document revision */
	readonly revisionNumber?:      string; /** The current revision number */
	readonly slideCount?:          string; /** The number of slides in the document */
	readonly versionDate?:         string; /** The date of the document version */
	readonly versionNumber?:       string; /** The document version number */
	readonly wordCount?:           string; /** The number of words in the document */
	}

export interface OITDocumentProperties
	{
	readonly general?:    OITDocumentGeneralProperties;
	readonly statistics?: OITDocumentStatistics;
	readonly other?:      OITStringMap;
	}

/*export enum OITZoomMode
	{
	pagewidth  = "pagewidth",  /// Set the zoom factor so the page will fill the view from left to right
	pageheight = "pageheight", /// Set the zoom factor so the page will fill the view from top to bottom
	page       = "page",       /// Set the zoom factor so the page in the will fit within the view
	mobile     = "mobile",     /// The default zoom mode for mobile devices. Spreadsheet, database and archive files are displayed at 100%, and other file types are zoomed so the page will fit within the width of the view
	}

export enum OITTextSearchDirection
	{
	up   = "up",
	down = "down"
	}

export enum OITTextSearchMode
	{
	string   = "string",   /// searchTerm is the literal string value to search for
	dtsearch = "dtsearch", /// searchTerm is a dtSearch expression. OIT will attempt to parse the expression to replicate the search results dtSearch would return. Not all dtSearch functionality is supported.
	lucene   = "lucene"    /// searchTerm is a lucene expression. OIT will attempt to parse the expression to replicate the search results lucene would return. Not all lucene functionality is supported.
	}*/

export interface OITHighlightConfig
	{
	style:             string;  /** The style to apply to each highlight. A CSS string that may include the color and/or background-color properties. Any other properties are ignored. */
	properties?:       object;  /** An object that contains additional data for the highlight. This object will be available to event handlers when events occur on this object, and may contain private data for the application's use. */
	visible?:          boolean; /** Indicates whether the highlight will be visible when it is first added */
	callbackFn?:       Function; 
	enableUserDelete?: boolean; /** Controls whether the user is able to delete the highlight by pressing the DELETE key while the highlight is active. Defaults to true.By default, highlights may be deleted by the user. */
	enableUserMove?:   boolean; /** Controls whether the user is able to move the highlight using the mouse, touch, or keyboard. This applies to rectangular and cell highlights only - text highlights may not be made moveable. By default, highlights may be moved by the user. */
	enableUserResize?: boolean; /** Controls whether the user is able to resize the highlight using the mouse, touch, or keyboard. By default, highlights may be resized by the user. */
	caseSensitive?:    boolean; /** Indicates whether the search should be case sensitive. Defaults to false. */
	mode?:             string /*OITTextSearchMode*/;  /** A string specifying the find mode. If not specified, the default of "string" is used. */
	}

export interface OITDocument
	{
	properties(): OITDocumentProperties|undefined;
	externalData(): object|undefined;
	setOptions( options: OITValueMap ): void;
	getOptions( optionName?: string ): any|[any];
	zoom( val?: number|string/*|OITZoomMode*/ ): number;
	zoomIn(): number;
	zoomOut(): number;
	rotate( angle?: number ): number;
	find( searchTerm: string, callbackFn: Function, caseSensitive?: boolean, direction?: string /*OITTextSearchDirection*/, startacc?: number, mode?: string /*OITTextSearchMode*/ ): void;
	findAll( searchTerm: string, callbackFn: Function, caseSensitive?: boolean, mode?: string /*OITTextSearchMode*/ ): void;
	getRawText( callbackFn: Function ): void;
	highlight( searchTerm: string, style: string, callbackFn: Function, caseSensitive?: boolean, properties?: object, mode?: string /*OITTextSearchMode*/ ): void;
	highlight( searchTerm: string, config: OITHighlightConfig ): void;
	select( range: any /*OITTextRange|OITCellRange|OITRectRange|Range|Selection*/, moveto?: boolean ): boolean;
	selectionType(): string /*OITRangeType*/;
	selectionMode( mode?: string /*OITRangeType*/ ): string /*OITRangeType*/;
	selectionStyle( style?: string ): string;
	selection(): OITTextRange|OITCellRange|OITRectRange|undefined;
	textSelection(): OITTextRange|undefined;
	cellSelection(): OITCellRange|undefined;
	rectSelection(): OITRectRange|undefined;
	getSelectedText(): string|undefined;
	textRange( domRange: Range ): OITTextRange|OITTextRange[]|undefined;
	textRange( domSelection: Selection ): OITTextRange|OITTextRange[]|undefined;
	textRange( acc: number, accend: number ): OITTextRange;
	textRange( cellRange: OITCellRange ): OITTextRange|OITTextRange[]|undefined;
	cellRange( sheet: number, c1: number, r1: number, c2: number, r2: number ): OITCellRange;
	cellRange( textRange: OITTextRange ): OITCellRange[];
	rectRange( page: number, x1: number, y1: number, x2: number, y2: number, units: string /*RectRangeUnits*/ ): OITRectRange;
	updateViewSize(): void;
	addEventListener( name: string, fn: Function ): void;
	removeEventListener( name: string, fn: Function ): void;
	}

export interface OITPageInfo
	{
	width:     number;   /** The width of the page, in pixels, at 100% zoom and 0 degrees rotation. Changing the zoom level or page rotation angle will not affect this value. */
	height:    number;   /** The height of the page, in pixels, at 100% zoom and 0 degrees rotation. Changing the zoom level or page rotation angle will not affect this value. */
	type:      string;
	typeEx:    string;
	acc?:      number;   /** The starting ACC for the text on the page */
	accend?:   number;   /** The ending ACC for the text on the page */
	notes?:    boolean;
	comments?: boolean;
	}

export interface OITPages
	{
	current(): number;
	pageName( index?: number ): string|undefined;
	count(): number;
	moveto( dest: any /*number|HTMLElement|Text|OITTextRange|OITCellRange|OITRectRange*/, callbackFn?: Function ): number;
	movetoNext(): number;
	movetoPrev(): number;
	info( index: number ): OITPageInfo;
	isDocumentText( acc: number, accend: number ): boolean;
	addEventListener( name: string, fn: Function ): void;
	removeEventListener( name: string, fn: Function ): void;
	}

export interface OITHighlights
	{
	add( style: string, range?: OITTextRange|OITCellRange|OITRectRange|Selection|Range, properties?: object, comment?: string ): number|number[]|undefined;
	addMultiple( style: string, rangeList: (OITTextRange|OITRectRange|OITCellRange|Range|Selection)[], properties?: object, options?: object ): number[];
	redact( range?: OITTextRange|OITCellRange|OITRectRange, properties?: object, label?: string ): number|number[]|undefined;
	apply( highlights: string, filterFunction?: Function ): void;
	remove( id?: number|number[] ): void;
	clear( filterFunction?: Function ): void;
	autoHighlight( type?: string, options?: object ): string;
	autoHighlightOptions( options?: object ): object;
	serialize( filterFunction: Function|undefined ): string;
	serialize( highlightid: number|number[] ): string;
	getJSON( highlightid: number ): string;
	setOptions( highlightid: number, options: object ): void;
	style( highlightid: number, style?: string ): string|undefined;
	label( highlightid: number, text?: string ): string|undefined;
	properties( highlightid: number, properties?: object ): object|undefined;
	property( highlightid: number, valueName: string, value?: any ): any;
	current(): number|undefined;
	active(): number|undefined;
	moveto( id?: number ): void;
	movetoNext( filterFn: Function ): void;
	movetoPrev( filterFn: Function ): void;
	type( id?: number ): string;
	range( id?: number ): OITTextRange|OITCellRange|OITRectRange;
	show( id?: number|number[] ): void;
	show( filterFn: Function ): void;
	hide( id?: number|number[] ): void;
	hide( filterFn: Function ): void;
	isHidden( id: number ): boolean;
	first(): number|undefined;
	last(): number|undefined;
	next( highlightId: number ): number|undefined;
	prev( highlightId: number ): number|undefined;
	activate( id?: number ): void;
	deactivate(): void;
	addEventListener( name: string, fn: Function ): void;
	removeEventListener( name: string, fn: Function ): void;
	}

export interface OITHyperlinks
	{
	addEventListener( name: string, fn: Function ): void;
	removeEventListener( name: string, fn: Function ): void;
	}

export interface OITModule {
	view:       OITView;
	document:   OITDocument;
	pages:      OITPages;
	highlights: OITHighlights;
	hyperlinks: OITHyperlinks;
	}
