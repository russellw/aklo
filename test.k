/* Block comments */
/**/
/***/
/****/
/*****/

//boolean literals
assert true==true
assert true!=false

//logic
assert !true==false
assert !false==true

assert !(false&&false)
assert !(false&&true)
assert !(true&&false)
assert (true&&true)

assert !(false||false)
assert (false||true)
assert (true||false)
assert (true||true)

//char literals
assert '\0'=='\x0'
assert '\1'=='\x01'
assert '\007'=='\x07'
assert 'A'=='\x41'
assert 'A'=='\u0041'
assert 'A'=='\U00000041'
assert '\\'=='\x5c'
assert '\a'=='\x7'
assert '\b'=='\x8'
assert '\t'=='\x9'
assert '\n'=='\xa'
assert '\v'=='\xb'
assert '\f'=='\xc'
assert '\r'=='\xd'
assert '\e'=='\x1B'

//integer literals
assert 'A'==65
assert 0x100==256
assert 0X1_00_00==65536
assert 0o100==64
assert 0b100==4

//floating-point literals
assert 1.0==1.00
assert 1.5e3==1500.0
assert 5e-3==0.005
assert 1e6==1_000_000.0

assert 1.0f==1.00f
assert 1.5e3f==1500.0f
assert 5e-3f==0.005f
assert 1e6f==1_000_000.0f
assert 1f==1.0f

//comparison
assert 1<2
assert !(1<1)
assert !(2<1)

assert 1<=2
assert 1<=1
assert !(2<=1)

assert !(1>2)
assert !(1>1)
assert 2>1

assert !(1>=2)
assert 1>=1
assert 2>=1

assert 1.9<2.9
assert !(1.9<1.9)
assert !(2.9<1.9)

assert -10<9

assert false<true
assert !(false<false)
assert !(true<false)

//arithmetic
assert -(-5)==5
assert 1+2==3
assert 10-9==1
assert 3*4==12
assert 10/3==3
assert 10%3==1
assert 3.0/2.0==1.5

//bitwise
assert ~0==-1
assert (0b1110&0b0111)==0b0110
assert (0b1110|0b0111)==0b1111
assert (0b1110^0b0111)==0b1001
assert 1<<8==256
assert 256>>2==64
assert 0xfff_ffff>>4==0xff_ffff
assert -1>>4==-1

assert !(false&false)
assert !(false&true)
assert !(true&false)
assert (true&true)

assert !(false|false)
assert (false|true)
assert (true|false)
assert (true|true)

assert true<<true==2
assert false|false==0
assert false^1==1

//operator precedence
assert 1+2*3==7
assert 1-2-3==-4
assert 999<1000&&1000<1001

//cast
assert 1.1!int==1
