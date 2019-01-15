%namespace TruthTree2.FOL.Input

%using TruthTree2.FOL.Logic

%union {
	public string stringval;
	public WFF formulaval;
	public Term termval;
	public List<Term> terms;
	public List<WFF> wffs;
}

%token <stringval> Identifier
%token <stringval> Variable
%token And
%token Or
%token Not
%token Iff
%token If
%token Forall
%token Exists
%token Equals
%token LParen
%token RParen
%token False

%type <formulaval> formula
%type <formulaval> atomicwff
%type <formulaval> complexwff
%type <wffs> formulalist
%type <termval> term
%type <terms> termlist


%start line

%%

line :			{ result = null; }
     | formula	{ result = $1; }
	 | error	{ result = null; }
	 ;

term : Identifier							{ $$ = new Constant($1); }
	 | Variable								{ $$ = new Variable($1); }
	 | LParen Identifier termlist RParen	{ $$ = new Function($2, $3.ToArray()); }
	 ;

termlist : termlist term	{ $1.Add($2); $$ = $1; }
		 |					{ $$ = new List<Term>(); }
		 ;

formula : atomicwff		{ $$ = $1; }
		| complexwff	{ $$ = $1; }
		;

atomicwff : False								{ $$ = new Contradiction(); }
		  | LParen Equals term term RParen		{ $$ = new Identity($3, $4); }
		  | LParen Identifier termlist RParen	{ $$ = new Predicate($2, $3.ToArray()); }
		  | Identifier							{ $$ = new Predicate($1); }
		  ;

complexwff : LParen Not formula RParen				{ $$ = $3.GetNegation(); }
		   | LParen Iff formula formula RParen		{ $$ = new Biconditional($3, $4); }
		   | LParen If formula formula RParen		{ $$ = new Conditional($3, $4); }
		   | LParen And formulalist RParen			{ $$ = new Conjunction($3.ToArray()); }
		   | LParen Or formulalist RParen			{ $$ = new Disjunction($3.ToArray()); }
		   | LParen Exists Variable formula RParen	{ $$ = new Existential(new Variable($3), $4); }
		   | LParen Forall Variable formula RParen	{ $$ = new Universal(new Variable($3), $4); }
		   ;

formulalist : formulalist formula	{ $1.Add($2); $$ = $1; }
			|						{ $$ = new List<WFF>(); }
			;

%%

Parser() : base(null) { }

private WFF result;

public static WFF parseString(string str)
{
    Scanner scanner = new Scanner();
    scanner.SetSource(str, 0);

    Parser parser = new Parser();
    parser.Scanner = scanner;

	parser.Parse();

    return parser.result;
}