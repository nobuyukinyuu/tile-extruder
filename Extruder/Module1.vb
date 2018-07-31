Imports System.Drawing

Module Module1
    Dim input, output As String
    Dim TileW, TileH As Integer
    Dim xPad As Integer = -1
    Dim xSpace As Integer = -1
    Dim yPad, ySpace As Integer

    Dim diagProcessLevel = 3  'How should we process diagonal extrusion? 
                                ' 0: Don't; 
                                ' 1: 1st pixel; 
                                ' 2: Avg of corner pixels
                                ' 3: Extrude corners (new default)

    Sub Main()
        If My.Application.CommandLineArgs.Count = 0 Then

            PrintUsage()
            End
        Else
            Dim args = My.Application.CommandLineArgs

            'Get help
            If args.Item(0) = "-h" Or args.Item(0).Contains("-help") Then
                Console.WriteLine(vbNewLine & "TileExtruder " & My.Application.Info.Version.ToString & " by Nobuyuki.")
                Console.WriteLine("Get the latest version at https://github.com/nobuyukinyuu/tile-extruder" & vbNewLine)
                PrintUsage()
                PrintHelp()
                End
            End If

            'Check to see if input file exists.
            If Not IO.File.Exists(args.Item(0)) Then
                Console.WriteLine("Error:  Input file does not exist.")
                End
            End If

            'Get input / output filenames
            input = args(0)
            If args.Count > 1 And (Not args(args.Count - 1).Contains("=")) Then
                output = args(args.Count - 1)
            Else 'Generate an output file based on the input file's name.
                output = IO.Path.GetDirectoryName(input) & IO.Path.GetFileNameWithoutExtension(input) & "_extruded" & IO.Path.GetExtension(input)
                Debug.Print(output)
            End If

            'Parse args
            For Each item In args
                Dim arg As String() = ParseArg(item)

                Try
                    Select Case arg(0)
                        Case "size"
                            TileW = arg(1)
                            TileH = arg(2)
                        Case "pad"
                            xPad = arg(1)
                            yPad = arg(2)
                        Case "space"
                            xSpace = arg(1)
                            ySpace = arg(2)
                        Case "diag"
                            diagProcessLevel = arg(1)

                    End Select

                Catch ex As Exception
                    Console.WriteLine("Error: Invalid parameter for argument " & Chr(34) & arg(0) & Chr(34))
                    End
                End Try
            Next
        End If


        'Manually get required arguments if they weren't specified.
        If TileW <= 0 Then
            Console.Write("Tile X Size: ")
            TileW = Int32.TryParse(Console.ReadLine(), 0)
            Console.Write("Tile Y Size: ")
            TileH = Int32.TryParse(Console.ReadLine(), 0)
            Console.WriteLine("")
        End If
        If xPad < 0 Then
            Console.Write("X Padding: ")
            xPad = Int32.TryParse(Console.ReadLine(), 0)
            Console.Write("Y Padding: ")
            yPad = Int32.TryParse(Console.ReadLine(), 0)
            Console.WriteLine("")
        End If
        If xSpace < 0 Then
            Console.Write("X Spacing: ")
            xSpace = Int32.TryParse(Console.ReadLine(), 0)
            Console.Write("Y Spacing: ")
            ySpace = Int32.TryParse(Console.ReadLine(), 0)
            Console.WriteLine("")
        End If

        Console.WriteLine("Processing...")
        Chooch()

        Dim infoReader As System.IO.FileInfo = My.Computer.FileSystem.GetFileInfo(output)
        If infoReader.Exists Then Console.WriteLine("Wrote " & infoReader.Length & " bytes.")
    End Sub

    Sub PrintUsage()
        Dim fg = Console.ForegroundColor

        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Write("Usage: ")
        Console.ForegroundColor = ConsoleColor.White
        Console.Write(Process.GetCurrentProcess.MainModule.ModuleName)
        Console.ForegroundColor = fg
        Console.Write(" " & "inputfile [args] [outputfile]")
        Console.WriteLine()
        Console.WriteLine()
    End Sub

    Sub PrintHelp()
        Dim fg = Console.ForegroundColor
        Dim bg = Console.BackgroundColor
        Console.Write("All args can take 1 or 2 values in px, and are in the form ")

        Console.BackgroundColor = ConsoleColor.DarkBlue
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("arg1=")
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write("x")
        Console.ForegroundColor = ConsoleColor.White
        Console.Write(",")
        Console.ForegroundColor = ConsoleColor.Red
        Console.Write("y")
        Console.ForegroundColor = fg
        Console.BackgroundColor = bg
        Console.Write("':" & vbNewLine)

        ArgLine("size", "Size of an individual tile in this set.")
        ArgLine("pad", "Amount each tile is looped/extruded past its border.")
        ArgLine("space", "Amount of empty space between each tile.")
        ArgLine("diag", "Diagonal pixel extrusion fill mode. (default: 3)", False)
        ArgLine("", " 0: No fill;")
        ArgLine("", " 1: Use first pixel; ")
        ArgLine("", " 2: Average corner pixels.")
        ArgLine("", " 3: Extrude corners diagonally")

        Console.WriteLine()
    End Sub

    Sub ArgLine(name As String, desc As String, Optional extraBlank As Boolean = True)
        Dim fg = Console.ForegroundColor
        Console.ForegroundColor = ConsoleColor.White
        Console.Write(vbTab & name & vbTab)
        Console.ForegroundColor = fg
        Console.Write(desc & vbNewLine)
        If extraBlank Then Console.Write(vbNewLine)
    End Sub

    'Parses an argument....
    Function ParseArg(arg As String) As String()
        Dim out(3) As String

        'Dim delimiters As String() = {"=", ","}
        Dim p As String() = arg.Split(New String({"=", ","}))
        Select Case p.Length
            Case Is >= 3
                Return p
            Case 2
                out(2) = p(1)
                out(1) = p(1)
                out(0) = p(0)
            Case 1
                out(0) = p(0)
                out(1) = "0"
                out(2) = "0"
        End Select

        Return out
    End Function

    'Processes the input file and returns an output file.
    Sub Chooch()
        Dim b As New Bitmap(input)

        'Determine the new bitmap's size by figuring out how many tiles we need to split up the input.
        Dim xTiles As Integer = b.Width / TileW, yTiles As Integer = b.Height / TileH
        Dim o As New Bitmap(xTiles * (TileW + xPad * 2 + xSpace * 2), yTiles * (TileH + yPad * 2 + ySpace * 2))

        'It's a trap! Windows likes 96dpi by default, but most files (Photoshop, etc) are saved as 76dpi!
        'So, we have to make sure our output resolution matches input, or else the dimensions will be hosed.
        o.SetResolution(b.HorizontalResolution, b.VerticalResolution)
        Dim g As Graphics = Graphics.FromImage(o)

        Dim tw, th As Integer
        tw = TileW/2
        th = TileH/2

        'Get all the source rects and chooch them from the original file.
        For y = 0 To yTiles - 1
            For x = 0 To xTiles - 1
                'Get some important src/dest coordinates.
                Dim srcRect As New Rectangle(x * TileW, y * TileH, TileW, TileH)
                Dim destx As Integer = x * (TileW + xSpace * 2 + xPad * 2) + xSpace + xPad
                Dim desty As Integer = y * (TileH + ySpace * 2 + yPad * 2) + xSpace + xPad

                'First, if we have any corner extruding to do, we need to apply a flat color for diagonals.
                If xPad > 0 Or yPad > 0 And diagProcessLevel > 0 Then
                    Dim fill As Color
                    If diagProcessLevel = 3 Then
                        'Extrude corner

                        'Top Left
                        fill = b.GetPixel(srcRect.X, srcRect.Y)
                        g.FillRectangle(New SolidBrush(fill), destx - xPad, desty - yPad, tw + xPad * 2, th + yPad * 2)

                        'Top Right
                        fill = b.GetPixel(srcRect.X + TileW - 1, srcRect.Y)
                        g.FillRectangle(New SolidBrush(fill), destx+tw - xPad, desty - yPad, tw + xPad * 2, th + yPad * 2)

                        'Bottom Left
                        fill = b.GetPixel(srcRect.X, srcRect.Y + TileH - 1)
                        g.FillRectangle(New SolidBrush(fill), destx - xPad, desty+th - yPad, tw + xPad * 2, th + yPad * 2)

                        'Bottom Right
                        fill = b.GetPixel(srcRect.X + TileW - 1, srcRect.Y + TileH - 1)
                        g.FillRectangle(New SolidBrush(fill), destx+tw - xPad, desty+th - yPad, tw + xPad * 2, th + yPad * 2)

                    Else
                        If diagProcessLevel = 2 Then
                            'Average the edge pixels.
                            Dim px1, px2, px3, px4 As Color
                            px1 = b.GetPixel(srcRect.X, srcRect.Y) 'UL
                            px2 = b.GetPixel(srcRect.X + TileW - 1, srcRect.Y) 'UR
                            px3 = b.GetPixel(srcRect.X, srcRect.Y + TileH - 1) 'LL
                            px4 = b.GetPixel(srcRect.X + TileW - 1, srcRect.Y + TileH - 1) 'LR

                            Dim aa As Integer = (Int(px1.A) + Int(px2.A) + Int(px3.A) + Int(px4.A)) / 4
                            Dim rr As Integer = (Int(px1.R) + Int(px2.R) + Int(px3.R) + Int(px4.R)) / 4
                            Dim gg As Integer = (Int(px1.G) + Int(px2.G) + Int(px3.G) + Int(px4.G)) / 4
                            Dim bb As Integer = (Int(px1.B) + Int(px2.B) + Int(px3.B) + Int(px4.B)) / 4

                            fill = Color.FromArgb(aa, rr, gg, bb)
                        ElseIf diagProcessLevel = 1 Then
                            fill = b.GetPixel(srcRect.X, srcRect.Y) 'UL
                        End If
                        g.FillRectangle(New SolidBrush(fill), destx - xPad, desty - yPad, TileW + xPad * 2, TileH + yPad * 2)
                    End If
                End If

                'Finally, copy the source tile and put it in the destination bitmap.
                g.DrawImage(b, destx, desty, srcRect, GraphicsUnit.Pixel)

                'Now, extrude strips from the source bitmap's sides and apply that too.
                If xPad > 0 Then
                    Dim strip As New Rectangle(srcRect.X, srcRect.Y, xPad, TileH)
                    'Left side
                    g.DrawImage(b, New Rectangle(destx, desty, -strip.Width, strip.Height), strip, GraphicsUnit.Pixel)

                    'Right side
                    strip.X += (TileW - xPad)
                    g.DrawImage(b, New Rectangle(destx + TileW + xPad, desty, -strip.Width, strip.Height), strip, GraphicsUnit.Pixel)
                End If
                If yPad > 0 Then
                    Dim strip As New Rectangle(srcRect.X, srcRect.Y, TileW, yPad)
                    'Top side
                    g.DrawImage(b, New Rectangle(destx, desty, strip.Width, -strip.Height), strip, GraphicsUnit.Pixel)

                    'Bottom side
                    strip.Y += (TileH - yPad)
                    g.DrawImage(b, New Rectangle(destx, desty + TileH + yPad, strip.Width, -strip.Height), strip, GraphicsUnit.Pixel)
                End If


            Next
        Next


        'Write output of o to file.
        If IO.File.Exists(output) Then
            Console.Write(IO.Path.GetFileNameWithoutExtension(output) & IO.Path.GetExtension(output) & _
                           " already exists.  Overwrite? (y/n): ")

            Dim response As ConsoleKeyInfo = Console.ReadKey()
            Console.Write(vbNewLine)

            'Get resopnse.
            If Char.ToLowerInvariant(response.KeyChar) = CChar("y") Then
                IO.File.Delete(output)
                o.Save(output)
            Else 'User didn't say yes......
                Console.WriteLine("Operation aborted.")
            End If
        Else
            o.Save(output)
        End If

        g.Dispose()
    End Sub
End Module



'' Lock the bitmap's bits.
'Dim rect As New Rectangle(0, 0, o.Width, o.Height)
'Dim bmpData As System.Drawing.Imaging.BitmapData = o.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, o.PixelFormat)

'' Get the address of the first line.
'Dim ptr As IntPtr = bmpData.Scan0

'' Declare an array to hold the bytes of the bitmap.
'Dim oData(Math.Abs(bmpData.Stride) * o.Height) As Byte

'' Copy the RGB values into the array.
'System.Runtime.InteropServices.Marshal.Copy(ptr, oData, 0, oData.Length)

'Dim colorValue As Color = Color.Purple
'        For i = 0 To oData.Length - 1 Step 4
'            oData(i) = colorValue.R
'            oData(i + 1) = colorValue.G
'            oData(i + 2) = colorValue.B
'            oData(i + 3) = colorValue.A
'        Next

'' Copy the RGB values back to the bitmap
'            System.Runtime.InteropServices.Marshal.Copy(oData, 0, ptr, oData.Length);

'' Unlock the bits.
'             o.UnlockBits(bmpData);
