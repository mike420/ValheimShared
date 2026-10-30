using System;
using UnityEngine;

namespace MagicMike.ModSettings;

// The Discord and PayPal logos for the links at the bottom of the panel, kept here so every mod that builds the panel has them
// without carrying the files. They are Shared/discord.png and Shared/paypal.png as raw 30x30 RGBA pixels, bottom row first, as
// reading a PNG would need Unity's image module, which the mods do not reference
internal static class LinkIcons
{
	private static Sprite s_discord;

	private static Sprite s_payPal;

	public static Sprite Discord => s_discord != null ? s_discord : s_discord = Load("Discord", DiscordPixels);

	public static Sprite PayPal => s_payPal != null ? s_payPal : s_payPal = Load("PayPal", PayPalPixels);

	private const int Size = 30;

	private static Sprite Load(string name, string pixels)
	{
		Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, mipChain: false) { name = "MagicMike " + name };
		texture.LoadRawTextureData(Convert.FromBase64String(pixels));
		texture.Apply();
		return Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f));
	}

	private const string DiscordPixels =
		"NSMc/zUjHP81Ixz/XE5V/7Ots//X1uT/2Nv5/8rO+/+6v/n/srf4/6yy9/+rsPj/qq/4/6mv+P+pr/j/qa/4/6mv+P+qr/j/qrD4/6yx9/+wtvf/ub75/8fL" +
		"+//Y2vr/3t3u/7+3wf9xYXH/OCYh/zUjHP81Ixz/NSMc/zgmIv+Xjpf/4OL6/5Wc+v9rdPf/WWT3/1Ne9/9TXff/UVz3/1Fb9/9RW/f/UVv4/1Fc+P9RXPj/" +
		"UVz4/1Fc+P9RXPj/UVz4/1Fb9/9RXPf/Ulz3/1Re9/9YY/f/ZG33/42V+P/V2vr/wLi//z0rK/81Ixz/NSMc/5CIlP/P0/v/XWj2/1Jc+P9XYfj/WmT4/1tl" +
		"+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/WWP4/1Ne+P9VXvf/srj4/7uyu/86JyX/" +
		"SzxB//Dx/v9gavb/WWP4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl" +
		"+P9bZfj/W2X4/1tl+P9aZPj/U173/9LW+/9mVmT/qqOr/6Ko9v9SXPj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/" +
		"W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/4WN+P/Qy9n/4NzX/1xm9f9aZPj/W2X4/1tl+P9bZfj/W2X4/1tl" +
		"+P9bZfj/WGL4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9XYfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/2Js9v/z9f//" +
		"397l/0xX9/9bZfj/W2X4/1tl+P9bZfj/WmT4/1Re+P9TXvf/Zm/2/1dh+P9cZvj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1Vf+P9rdPX/VF/3/1Ne" +
		"+P9aZPj/W2X4/1tl+P9bZfj/W2X4/1Jc9v/m6v3/0dLs/1Fc+P9bZfj/W2X4/1tl+P9WYPj/Vl/3/4CI9v/Bxfn/9/j8/5GY9f9VX/j/WGL4/1Vf+P9TXvj/" +
		"U174/1Rf+P9XYvj/WGL4/32G9f/4+fz/y875/5CW9v9YYvb/VF74/1tl+P9bZfj/W2X4/1Fb+P/N0vr/ycz0/1Nd+P9bZfj/W2X4/1Zg+P9ze/X/zND4//z8" +
		"/f///////////9/h+P9MWPX/Zm/2/3yE9/+Ij/f/iI/3/36H9v9rdPb/WGP2/7m+9////////////////v/X2vr/hIz2/1Nd9/9bZfj/W2X4/1Nd+P+8wvn/" +
		"wsb4/1Ne+P9bZfj/W2X4/4yU9v/29vz/////////////////zND2/6mu9f/U1/r/8PL9//7//f/////////+/////v/19vz/2dz6/66z9v+2u/X/////////" +
		"///////////9/6yx9v9TXvf/W2X4/1Ne+P+yuPn/vsL3/1Re+P9bZfj/W2X4/7S4+P//////////////////////+vr9//7+////////////////////////" +
		"///////////////////////////29/3//////////////////////+rr/P9QW/X/W2X4/1Re+P+ts/n/vMD3/1Rf+P9bZfj/W2X4/7e8+P//////////////" +
		"/////////////////v/2+Pz//v7+//////////////////////////7/9/f8//39/v///////////////////////////+zt/f9TXfb/W2X4/1Re+P+qsfn/" +
		"u7/2/1Rf+P9bZfj/W2X4/7O3+P/////////////////////////9/56j9v9ocfb/kpj1/////v///////////////v+qr/b/aXP2/4mQ9v/9/f3/////////" +
		"/////////////+nq/f9QW/b/W2X4/1Ve9/+psPr/ur/2/1Rf+P9bZfj/W2X4/6ar9///////////////////////zND5/1Zg+P9XYfj/SVX3/8nL9///////" +
		"/////9rd+/9bZff/V2L4/0pV9/+4vfb//////////////////////9fZ+v9QWvf/W2X4/1Ve+P+or/r/u7/3/1Rf+P9bZfj/W2X4/5KZ9//+/v7/////////" +
		"////////vsP5/1hi+P9bZfj/T1r4/7K39f///////////9DS+v9bZff/W2T4/1Fb+P+gpvT//////////////////////7a69v9SXfj/W2X4/1Ve+P+pr/r/" +
		"u7/3/1Rf+P9bZfj/W2X4/3mB9//29v7/////////////////9Pb8/3Z+9P9SXPb/Z3H0//b3+/////////////n5/f+Civb/Ul32/2Bq9P/u7/v/////////" +
		"/////////////4WM9f9WYfj/W2X4/1Ve+P+pr/r/ur/2/1Rf+P9bZfj/W2X4/1lk9v/q6/v///////////////////////T1+//O0fn/8PH7////////////" +
		"///////////4+f3/z9L5/+vt+//////////////////////////9/1Re9v9bZfj/W2X4/1Ve+P+pr/r/u7/3/1Rf+P9bZfj/W2X4/1Fc+P+zuPb/////////" +
		"////////////////////////////////////////////////////////////////////////////////////////ycz3/1Re+P9bZfj/W2X4/1Re9/+psPn/" +
		"vMD3/1Rf+P9bZfj/W2X4/1lj+P9hbPT////+////////////////////////////////////////////////////////////////////////////////////" +
		"//////7/g4v2/1pk+P9bZfj/W2X4/1Re9/+qsfr/vsL4/1Re+P9bZfj/W2X4/1tl+P9TXfj/u8D2////////////////////////////////////////////" +
		"///////////////////////////////////////////Z2/r/YGj3/1tl+P9bZfj/W2X4/1Re+P+ts/n/w8f3/1Ne+P9bZfj/W2X4/1tl+P9bZfj/bnb1//Hy" +
		"/P////////////////////////////////////////////////////////////////////////////////+FjfX/V2H4/1tl+P9bZfj/W2X4/1Ne+P+zuPn/" +
		"ycz0/1Jd+P9bZfj/W2X4/1tl+P9bZfj/WGL3/6Gn9f/39/z/////////////////9fb9//P0/P/5+f3/+fr9//P0/f/z9fz//////////////////P39/7/D" +
		"9/9PWff/W2X4/1tl+P9bZfj/W2X4/1Nd+P+9wvn/0tPu/1Fb+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1Rg9/9vd/b/n6X3/83R+f/x8vv/kZj1/2dw9v9wefb/" +
		"cHn2/2Zw9v94gfT/+fn8/9fZ+v+mrPj/dX72/1Zg9/9bZvj/W2X4/1tl+P9bZfj/W2X4/1Fb+P/P0/r/4eDk/0xX9/9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl" +
		"+P9XYvj/UVz4/1Vf9v9ibPf/WmT4/1hi+P9WYfj/VmH4/1hi+P9VX/j/Zm/3/1hi9/9RXPf/VmD4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1Jc9v/n6v3/" +
		"4N3X/15o9P9aZPj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9YY/j/WmX4/1tl+P9bZfj/W2X4/1tl+P9bZfj/WGL4/1pk+P9bZfj/W2X4/1tl" +
		"+P9bZfj/W2X4/1tl+P9bZfj/W2X4/2Vv9v/z9f//pZ6k/6mu9v9SXPj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/" +
		"W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/4mR9//Gwc//Rjc4//Lz/f9lbvX/WGL4/1tl+P9bZfj/W2X4/1tl" +
		"+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9ZY/j/VF/3/9nc+v9fUFz/" +
		"NSMc/4J7g//W2vr/Y232/1Fb+P9WYPj/WWP4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl" +
		"+P9aZPj/WGL4/1Jc+P9ZY/b/v8T6/62lrf84JyL/NSMc/zcmIP+If4b/5Ob3/6Cm+P9zfPf/XWj3/1Zh9/9UXvf/Ul33/1Jd9/9SXff/Ulz3/1Jc9/9SXPf/" +
		"Ulz3/1Jc9/9SXPf/Ulz3/1Jd+P9TXff/U173/1Zg9/9dZ/f/bHb3/5mg+f/d4fr/q6Kr/zooJf81Ixz/NSMc/zUjHP81Ixz/U0VH/6afpP/NzNT/29zz/9TX" +
		"+v/Hy/n/v8T6/7q/+f+4vfn/trv3/7a79/+2u/f/trr3/7a89/+4vPj/uLz4/7q++v+/w/r/x8r5/9LW+//d3/b/1tTh/6ylrf9fUFn/NiQe/zUjHP81Ixz/";

	private const string PayPalPixels =
		"NSMc/zUjHP81Ixz/OCUf/0o7PP9yZm//npag/7i0wP/GxND/zMrZ/8/M3f/Qz9//0dDi/9HQ4f/R0OL/0dDi/9HQ4v/Rz+L/0c/i/8/N3v/Oy9v/yMbU/764" +
		"x/+nnqz/fG58/1RES/89LCf/NSMc/zUjHP81Ixz/NSMc/zMhGf9DMzD/j4WN/9bW7f/Cx///l5/6/4KK+f91fvj/bXb3/2hy9v9ocff/Z3D3/2Zw9/9mcPf/" +
		"ZnD3/2Zw9/9ncPf/Z3D3/2hy9v9tdvb/dH34/3+I+f+Sm/v/uMD+/9zd9/+nnav/SDc8/zIgGf81Ixz/NyYg/0c2NP+vqa//wcX9/2x2+v9JVfj/Tln4/1Jb" +
		"+P9UXfj/VmH4/1dh+P9XYvj/V2L4/1hi+P9YYvj/WGL4/1hi+P9XYvj/V2L4/1dh+P9XYfj/VF/4/1Jc+P9PWvj/SlX4/15o+P+0uv//xsDL/1pISf86KCT/" +
		"TT5A/5CGhv+ytvT/W2X9/0xY9/9dZvj/XGb4/1tl+P9bZfj/W2X4/1xl+P9dZPn/XWT5/11k+f9dZPn/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl" +
		"+P9cZfj/XWb4/1Bb9/9VYPr/q7H+/6adp/9mVmH/g3l//6Ogwf94gv//TVj4/1pl+P9bZfj/W2X4/1tl+P9bZfj/W2X5/1ll9/9Wafb/Vmn2/1Zp9v9WafX/" +
		"W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/11m+P9NWPf/a3j//62t1f+onKf/mpSk/5yc1f9pdP//Ul34/1xm+P9bZfj/W2X4/11n" +
		"+v9dZ/z/YWL+/05y9f8sjeP/KY7g/ymN4P8njt7/WGj2/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/11m+P9SXfj/VmL9/5SX4/+xq8r/" +
		"qafH/5WZ4/9gbP7/VV/4/1xl+P9bZfn/W2T3/1ll9P9ZZPT/XmD3/0xv7f8WmtT/DaPT/w6h0/8KpND/SXTu/1xk+f9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl" +
		"+P9bZfj/W2X4/1tl+P9ZY/j/VF/6/4CF7P+npuD/uLjn/5OZ8P9ZZPr/VmD4/1tl9/9dZ/3/V2Hv/0BRv/85S63/O0uw/zxKr/8jZKv/GJbT/xib2P8VnNb/" +
		"OYHo/19i+f9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9bZfj/W2X4/1tl+P9cZfj/UFv5/3Z99P+ipPD/u7/1/5GX9v9WYfn/V2H4/1tl9v9gaf//V2Lu/yxA" +
		"j/8bM2r/HTZv/x80b/8lOH3/GY3L/xec2P8Undb/KYzi/2Rd/P9eYvr/XmL6/15i+v9dY/n/XGT5/1tl+P9bZfj/W2X4/1tl+P9cZfj/UFv4/3R99/+fpPX/" +
		"u7/5/46U9/9UX/j/WGL4/1tl9/9cZ/v/X2j//zZIp/8iOHr/JDt//yU6f/8lPoL/GoXE/xeb1/8XnNb/HZXa/1tm+P9VavX/VWr1/1Zo9v9bZfn/X2L7/1xj" +
		"+f9bZfj/W2X4/1tl+P9bZvj/UFz5/3R89/+eo/X/uL34/4uS9/9UX/j/WWP4/1tl+P9bZvf/Y2v//zxMsv8jOnz/JDp+/yU6f/8mOX//G36+/xeZ1v8Xmtf/" +
		"F5vX/yuM4P8qjN//Ko3g/yuL4f82g+f/TnDy/15h+v9eY/n/W2X4/1tl+P9cZvj/UVz4/3R89/+dofb/uL35/4qS+P9UX/f/WWP4/1tl+P9bZfj/Ymr//0FP" +
		"vf8mPIL/JDp+/yU7gP8mNXr/G3u6/xae2f8WoNz/FqDc/wyn1/8Optb/DqLU/w6h0/8RodP/IJPb/0Z27v9fYfr/XGT4/1tl+P9cZvj/UVz4/3N79/+coPb/" +
		"t7v4/4qR9/9UXvj/WWP4/1tl+P9bZfj/YGn//0dUyv8pPon/JDt//yU7gP8mMnj/HXCv/xeb1/8Xndn/F53Z/xed2v8WoNz/FaLf/xah3P8UnNX/DqHS/yCU" +
		"2/9Ob/L/YGD6/1tl+P9cZvj/UVz4/3N79/+bn/X/trv4/4mR9/9UXvj/WWP4/1tl+P9bZff/YGr//0xZ1f8sQJD/JTt+/yU7gP8mN3z/Ik6R/x9lpf8fZqf/" +
		"H2an/x9mp/8ecLD/G4bF/xee2f8Xod7/FpvW/xSd1f83g+f/ZF/8/1tl+P9bZvj/UVz4/3R79/+bn/X/trv4/4mR9/9UXvj/WWP4/1tl+P9bZff/YGn//1Bd" +
		"4P8vQpb/JDt+/yU7gP8lO4D/Jjd8/yY1ev8mNXn/JjV5/yc1ef8mOH3/JEWJ/x9pqv8YmdX/FKLd/xOe1P8wh+P/YmD7/11k+f9bZvj/UVz4/3R79/+cn/b/" +
		"t7v4/4mR9/9UXvj/WWP4/1tl+P9bZfj/Xmj//1Vh6/8zRp7/JDp+/yU7f/8lO4D/Jjl+/yY3fP8mN3z/Jjd8/yY3fP8mNXv/JzJ4/yY4ff8fZ6f/Fp/b/xOg" +
		"1v8xhuT/Yl/8/1xk+P9bZvj/UVz4/3R79/+cn/b/trv4/4mR9/9UXvj/WWP4/1tl+P9bZfj/XGb9/1lk9v82SaX/JDp+/yU7f/8lO4D/JTuA/yU8gf8lPIH/" +
		"JTyB/yU8gf8lPIH/JTuA/yY2e/8lP4P/GX+6/xSn4v9Aeun/Y1/7/1tl+P9cZvj/UVz4/3N79/+bn/X/t7v5/4qR+P9UXvj/WWP3/1tl+P9bZfj/W2X5/2Bp" +
		"/v85S6z/JTp+/yU7f/8lO4D/JTuA/yU7gP8lO4D/JTuA/yU7gP8lO4D/JTuA/yU6f/8lNnv/IFyd/yyX7/9YaPf/XmL5/1tl+P9cZvj/UVz4/3N79/+bn/X/" +
		"ub35/4qS+P9UXvj/WWT3/1tm+P9bZfj/WmT3/2Vt//89TbD/JTp+/yU7f/8lO4D/JTuA/yU7gP8lO4D/JTuA/yU7gP8lO4D/JTuA/yU7f/8jN3r/LEiZ/1Fv" +
		"9P9iY///WmX3/1tl+P9cZvj/UVz4/3N79/+boPb/ub34/4uS9/9UX/j/WWT4/1tl+P9bZfj/WmT2/2Zt//9BUbv/JjyC/yQ7fv8lO4D/JTuA/yU7gP8lO4D/" +
		"JTuA/yU7gP8lO4D/JTuA/yQ7f/8hOHj/MkOc/2Bj/P9fZ/7/W2X4/1tl+P9cZvj/UVz5/3N89/+bofb/ur/5/46U9/9UX/j/WGL4/1tl+P9bZfj/WmT2/2Ns" +
		"//9KWM7/KT6J/yM6ff8lO4D/JTuA/yU7gP8lO4D/JTuA/yU7gP8lO4D/JTuA/yQ6f/8iOHr/N0em/2Rr//9bZvj/W2X4/1tl+P9bZvj/UFz5/3R89/+eo/T/" +
		"u7/1/5GX9v9XYfn/V2H4/1tl+P9bZfj/W2X3/2Fq//9SXuL/LEGP/yI6ff8lO4D/JTuA/yU7gP8lO4D/JTuA/yU7gP8lO4D/JjuC/yM5ff8lO4H/SFbM/2Fq" +
		"//9bZff/W2X4/1tl+P9bZfj/T1r4/3R89/+gpPT/tbjk/5OZ7/9bZPr/VmD4/1tl+P9bZfj/W2X4/15n/f9bZPj/LkOW/yI6e/8lO3//JTuA/yU7gP8lO4D/" +
		"JTuA/yU7gP8kOn//IDd1/x82df87TbL/XGb5/1xm+/9bZfj/W2X4/1tl+P9bZfj/UFz5/3Z88/+hpO3/qKfF/5aZ4v9hbP3/VV/4/1xl+P9bZfj/W2X4/1tl" +
		"+P9ja///LkOT/xo0av8eNW//HjVw/x41cP8eNXD/HjVw/x41cP8fNXP/Jz2F/z9PuP9YZPP/XWf+/1tl+P9bZfj/W2X4/1tl+P9YY/j/VV/6/4CF6/+npt7/" +
		"nJel/52d1P9oc///U134/1xm+P9bZfj/W2X4/1tl+P9faf//SFbO/z1Ntv8/Trf/P063/z9Ot/8/Trf/P063/z9Ot/9EUsP/T13g/1pk9/9dZ/z/W2X4/1tl" +
		"+P9bZfj/W2X4/11n+P9SXPf/V2L9/5eZ4f+yq8b/g3l9/6SgvP97g///TVj4/1tl+P9cZvj/W2X4/1tl+P9bZfn/XGb3/1pk9v9aZPb/WmT2/1pk9v9aZPb/" +
		"WmT2/1pk9v9cZfn/XWb8/11m/P9bZfj/W2X4/1tl+P9bZfj/W2X4/1xm+P9NV/f/cHr//7Cv1f+lmqT/Sjs7/46Fgf+3u/L/Xmr8/0tW9/9cZfj/XWb4/1tl" +
		"+P9bZfj/XWf6/11n+v9dZ/r/XWf6/11n+v9dZ/r/XWf6/11n+v9dZvr/XGX5/1tl+P9bZfj/W2X4/1tl+P9cZvj/XGb4/05Z9/9XY/v/rrX8/56Vm/9dTlT/" +
		"NiUf/0ExLf+moaP/xcn7/3V+/P9LWPf/S1f4/1Fa+P9SXfj/U134/1Rf+P9VX/j/VWD4/1Zg+P9WYPj/VWD4/1Zg+P9VX/j/VF/4/1Re+P9TXvj/U134/1Ba" +
		"+P9NV/j/Slb2/2Vy+P+6wP//v7fB/1FBQf85JyL/NSMc/zMhGv8/Lir/g3l8/9PR4//Jzv7/oaj7/4yT+v9+hvj/eID4/3R89/9xevf/b3j2/2549v9uePb/" +
		"b3j2/2949v9wevb/cXr3/3R8+P94gPn/fYb4/4qR+/+epPr/xcr//9nb7/+VjJb/RDM0/zMiGv81Ixz/NSMc/zUjHP81Ixz/NyUd/0c3Nf9pXWD/jIaM/6qn" +
		"sP/BvMf/yMXQ/8vK1P/PzNn/0c7b/9PQ3P/Tz9z/08/c/9DO2//Mydf/y8rX/8vK1v/Jx9P/wLvH/62psv+Uipb/bmFr/0o5O/84JiD/NSMc/zUjHP81Ixz/";
}
