namespace System.Runtime.CompilerServices;

public static class IsExternalInit
{
	/*
	 * You might think this class isn't used. And you'd be right!
	 * But if you delete this file, the project will refuse to compile.
	 * This is because IsExternalInit is only included in net5 and above.
	 * So we have to create this dummy file to make it happy.
	 * Yay.
	 */
}