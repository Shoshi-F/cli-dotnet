using System.CommandLine;
#region הפרויקט
var bundleCommand = new Command("bundle", "Bundle code files to a single file");
var createRspCommand = new Command("create-rsp", "Create a response file for the bundle command");

var outputOption = new Option<FileInfo>(new[] { "--output", "-o" }, "File path and name");
var languageOption = new Option<string[]>(new[] { "--language", "-l" }, "List of programming languages");
languageOption.AllowMultipleArgumentsPerToken = true;
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Add a note to the bundle file");
var sortOption = new Option<string>(new[] { "--sort", "-s" }, getDefaultValue: () => "abc", "Sort files by name (abc) or language");
var removeEmptyLinesOption = new Option<bool>(new[] { "--remove-empty-lines", "-r" }, "Remove empty lines from code before bundling");
var authorOption = new Option<string>(new[] { "--author", "-a" }, "Add author");

bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);
createRspCommand.AddOption(outputOption);
bundleCommand.SetHandler((output, languages, note, sort, removeEmptyLines, author) =>
{
    try
    {
        List<string> codeFiles = new List<string>();

        // אפשרות לאריזת כל הקבצים של כל השפות
        if (languages.Contains("all"))
        {
            codeFiles.AddRange(Directory.GetFiles(".", "*", SearchOption.AllDirectories));
        }
        else
        {
            foreach (string language in languages)
            {
                codeFiles.AddRange(Directory.GetFiles(".", $"*.{language}", SearchOption.AllDirectories));
            }
        }

        // בדיקה לקיום קבצים בשפות המבוקשות
        foreach (var language in languages)
        {
            var files = Directory.GetFiles(".", $"*.{language}", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                Console.WriteLine($"ERROR: No files found for language: {language}");
            }
        }

        if (codeFiles.Count == 0)
        {
            Console.WriteLine("ERROR: No files found to concatenate");
            return;
        }

        // מיון הקבצים לפי abc ברירת מחדל
        codeFiles = sort == "language"
            ? codeFiles.OrderBy(f => Path.GetExtension(f)).ToList()
            : codeFiles.OrderBy(f => f).ToList();

        using (var outputFile = File.CreateText(output.FullName))
        {
            outputFile.WriteLine("// Author: " + author);
            foreach (var file in codeFiles)
            {
                if (note)
                {
                    outputFile.WriteLine("// File: " + Path.GetFileName(file));
                    outputFile.WriteLine("// Location: " + Path.GetFullPath(file));
                    outputFile.WriteLine();
                }

                var fileContent = File.ReadAllText(file);
                if (removeEmptyLines)
                {
                    fileContent = string.Join("", fileContent.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
                }


                outputFile.WriteLine(fileContent);
                outputFile.WriteLine();
            }
        }
        Console.WriteLine("Bundle created successfully: " + output.FullName);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creating bundle: " + ex.Message);
    }
}, outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);





createRspCommand.SetHandler(() =>
{
    try
    {
        //מאתחל את קובץ התגובה
        var responseFile = new FileInfo("responseFile.rsp");
        Console.WriteLine("Enter values for the bundle command:");
        using (StreamWriter rspWriter = new StreamWriter(responseFile.FullName))
        {
            rspWriter.WriteLine("bundle");
            Console.Write("Output file path: ");
            var Output = Console.ReadLine();
            //בדיקה שהמשתמש אכן הכניס קלטים
            while (string.IsNullOrWhiteSpace(Output))
            {
                Console.Write("Enter the output file path: ");
                Output = Console.ReadLine();
            }
            //כתיבת נתיב קובץ ה-output לקובץ התגובה
            rspWriter.WriteLine($"--output {Output}");
            Console.Write("Languages (comma-separated): ");
            var languages = Console.ReadLine();
            //שוב בדיקה שיש קלט מהמשתמש
            while (string.IsNullOrWhiteSpace(languages))
            {
                Console.Write("Please enter at least one programming language: ");
                languages = Console.ReadLine();
            }
            //כתיבת הקלטים לקובץ התגובה
            rspWriter.WriteLine($"--language {languages}");
            Console.Write("Add note (y/n): ");
            rspWriter.WriteLine(Console.ReadLine().Trim().ToLower() == "y" ? "--note" : "");
            Console.Write("Sort by (abc or language): ");
            rspWriter.WriteLine($"--sort {Console.ReadLine()}");
            Console.Write("Remove empty lines (y/n): ");
            rspWriter.WriteLine(Console.ReadLine().Trim().ToLower() == "y" ? "--remove-empty-lines" : "");
            Console.Write("Author: ");
            rspWriter.WriteLine($"--author {Console.ReadLine()}");
        }
        //הודעה ע"כ שקובץ התגובה נוצר בהלחה עם הנתיב שלו
        Console.WriteLine("Response file created successfully: " + responseFile.FullName);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creating response file: " + ex.Message);
    }
});

//פקודת השורש של הפעולה
var rootCommand = new RootCommand("Root command for file Bundler CLI");
//הוספת הפקודות של הפעולה עצמה לשורש
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
//הפעלה
rootCommand.InvokeAsync(args);
#endregion
