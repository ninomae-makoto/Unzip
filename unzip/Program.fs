open System.IO.Compression
open System.IO
open System.Text.RegularExpressions
open System.Reflection

/// <summary>
/// <para>受け取ったzipファイルの解凍を行う</para>
/// <para>zipファイル直下がフォルダ一つの時はそのフォルダ名で解凍。それ以外はzipファイル名で解凍</para>
/// <para>引数 filepaths : 解凍する複数ファイルのパス</para>
/// <para>戻り値　解凍したzipファイルの数</para>
/// </summary>
let unzip (filepaths : string[]) : int =

    let mutable extractCount:int = 0

    for filepath: string in filepaths do
        printfn "%s" filepath
        if File.Exists filepath then
            use archive:ZipArchive = ZipFile.Open( filepath, ZipArchiveMode.Update ) 
            
            // F#用に変換
            let archiveEntries : ZipArchiveEntry[] = [| for entry in archive.Entries -> entry |]
            
            // archiveEntries が条件に一致するものを取得
            // 条件：zipファイル直下がフォルダのみ
            let pattern = Regex("/{\w}+")
            let currentFolder  = 
                 Array.filter (
                        fun (entry:ZipArchiveEntry) -> 
                            pattern.IsMatch( entry.FullName ) = false && entry.Length = 0L
                 ) archiveEntries
            
            try      
                if currentFolder.Length = 1 then
                    // zipファイル直下が1つ and フォルダのとき 実行パスを解凍先へ
                    archive.ExtractToDirectory( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) )
                else
                    // それ以外 zipファイル名を解凍先名に
                    archive.ExtractToDirectory( Path.GetFileNameWithoutExtension(filepath) )
            with // 解凍先に同名ファイルが存在した場合など
                | :? System.IO.IOException as ex -> printfn "解凍エラー：%s" ex.Message

            extractCount <- extractCount + 1
            
    extractCount

/// <summary>
/// エントリポイント
/// <para>引数 filepaths : コマンドライン引数</para>
/// <para>戻り値　0:正常終了 -1:想定外のエラー(要するにバグ)</para>
/// </summary>
[<EntryPoint>]
let main (filepaths : string[]) : int =
    try      
        let extractPath:string = ""
        let extractCount = unzip(filepaths)

        printfn "%d個 解凍しました" extractCount

        0
    with
        | ex -> printfn "予期せぬエラー：%s" ex.Message; -1