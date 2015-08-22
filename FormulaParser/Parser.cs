using System.Collections.Generic;
using System.Linq.Expressions;
using FormulaParser.Helpers;

//$namespace=FormulaParser



using System;

namespace FormulaParser {



public class Parser {
	public const int _EOF = 0;
	public const int _number = 1;
	public const int _identifier = 2;
	public const int _self = 3;
	public const int maxT = 14;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public Expression Output { get; set; }
public Expression BaseExpression { get; set; }



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void Formula() {
		Expression e; 
		Term(out e);
		Expression e1; 
		while (la.kind == 4 || la.kind == 5) {
			Func<Expression, Expression, Expression> op = null; 
			if (la.kind == 4) {
				Get();
				op = ExpressionsHelper.Add; 
			} else {
				Get();
				op = ExpressionsHelper.Subtract; 
			}
			Term(out e1);
			e = op(e, e1); 
		}
		this.Output = e; 
	}

	void Term(out Expression e) {
		Factor(out e);
		Expression e1; 
		while (StartOf(1)) {
			Func<Expression, Expression, Expression> op = null; 
			if (la.kind == 6) {
				Get();
				op = ExpressionsHelper.Multiply; 
			} else if (la.kind == 7) {
				Get();
				op = ExpressionsHelper.Divide; 
			} else if (la.kind == 8) {
				Get();
				op = ExpressionsHelper.Mod; 
			} else {
				Get();
				op = ExpressionsHelper.Exponent; 
			}
			Factor(out e1);
			e = op(e, e1); 
		}
	}

	void Factor(out Expression e) {
		e = null; 
		if (la.kind == 1) {
			Numeric(out e);
		} else if (la.kind == 3) {
			Get();
			e = BaseExpression; 
		} else if (la.kind == 2) {
			FunctionOrProperty(out e);
		} else if (la.kind == 10) {
			Get();
			int sign = 1; 
			if (la.kind == 5) {
				Get();
				sign = -1; 
			}
			Formula();
			e = ExpressionsHelper.SignMultiply(this.Output, sign); 
			Expect(11);
		} else SynErr(15);
	}

	void Numeric(out Expression e) {
		Expect(1);
		e = Expression.Constant(Convert.ToDouble(t.val)); 
	}

	void FunctionOrProperty(out Expression e) {
		bool propertyAccess = true; string functionName; List<Expression> args = null; Expression temp = null; 
		Name(out functionName);
		if (la.kind == 10) {
			Get();
			propertyAccess = false; 
			Arg(out temp);
			args = new List<Expression>(); args.Add(temp); 
			while (la.kind == 12) {
				Get();
				Arg(out temp);
				args.Add(temp); 
			}
			Expect(11);
		}
		if (propertyAccess) e = ExpressionsHelper.PropertyAccess(BaseExpression, functionName); else e = ExpressionsHelper.FunctionCall(functionName, args); 
	}

	void Name(out string name) {
		Expect(2);
		name = t.val; 
	}

	void Arg(out Expression e) {
		e = null; 
		if (StartOf(2)) {
			Factor(out e);
		} else if (la.kind == 13) {
			String(out e);
		} else SynErr(16);
	}

	void String(out Expression e) {
		Expect(13);
		Get();
		Expect(13);
		e = Expression.Constant(t.val); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Formula();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x},
		{_x,_T,_T,_T, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "number expected"; break;
			case 2: s = "identifier expected"; break;
			case 3: s = "self expected"; break;
			case 4: s = "\"+\" expected"; break;
			case 5: s = "\"-\" expected"; break;
			case 6: s = "\"*\" expected"; break;
			case 7: s = "\"/\" expected"; break;
			case 8: s = "\"%\" expected"; break;
			case 9: s = "\"^\" expected"; break;
			case 10: s = "\"(\" expected"; break;
			case 11: s = "\")\" expected"; break;
			case 12: s = "\",\" expected"; break;
			case 13: s = "\"\\\"\" expected"; break;
			case 14: s = "??? expected"; break;
			case 15: s = "invalid Factor"; break;
			case 16: s = "invalid Arg"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}