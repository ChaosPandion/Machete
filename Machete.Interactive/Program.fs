namespace Machete.Interactive

module Program =

    let parseJQuery () =
        let url = "http://code.jquery.com/jquery-1.4.4.js"
        let text = (new System.Net.WebClient()).DownloadString(url)
        let engine = new Machete.Engine()
        let r = engine.ExecuteScript text
        System.Console.WriteLine (r)

    let main () =
        //parseJQuery ()
        Interactive.initialize ()

    main()