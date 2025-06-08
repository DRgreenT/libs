using System.Security.Cryptography;
using System.IO;
using System;
using System.Collections.Generic;

/// <summary>
/// Extended file information and utility class for file operations 
/// </summary>
/// <remarks>
/// Version 2025-06-08
/// </remarks>
public class FileInfoExt
{
    /// <summary>
    /// Gets the underlying <see cref="FileInfo"/> object.
    /// </summary>
    public FileInfo FileInfo { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileInfoExt"/> class.
    /// </summary>
    /// <param name="path">Full path to the file.</param>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    public FileInfoExt(string path)
    {
        FileInfo = new FileInfo(path);
        if (!FileInfo.Exists)
            throw new FileNotFoundException("File not found.", path);
    }

    /// <summary>Gets the name of the file.</summary>
    public string Name { get { return FileInfo.Name; } }

    /// <summary>Gets the file extension.</summary>
    public string Extension { get { return FileInfo.Extension; } }

    /// <summary>Gets the full file path.</summary>
    public string FullPath { get { return FileInfo.FullName; } }

    /// <summary>Gets the file name without the extension.</summary>
    public string FileNameWithoutExtension { get { return Path.GetFileNameWithoutExtension(FileInfo.Name); } }

    /// <summary>Gets the file size in bytes.</summary>
    public long Size { get { return FileInfo.Length; } }

    /// <summary>Gets the file creation timestamp.</summary>
    public DateTime Created { get { return FileInfo.CreationTime; } }

    /// <summary>Gets the last access timestamp.</summary>
    public DateTime LastAccessed { get { return FileInfo.LastAccessTime; } }

    /// <summary>Gets the last modification timestamp.</summary>
    public DateTime LastModified { get { return FileInfo.LastWriteTime; } }

    /// <summary>Gets the directory name where the file is located.</summary>
    public string DirectoryName { get { return FileInfo.DirectoryName; } }

    /// <summary>Gets whether the file is hidden.</summary>
    public bool IsHidden { get { return (FileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden; } }

    /// <summary>Gets whether the file is read-only.</summary>
    public bool IsReadOnly { get { return FileInfo.IsReadOnly; } }

    /// <summary>Gets whether the file is marked as a system file.</summary>
    public bool IsSystem { get { return (FileInfo.Attributes & FileAttributes.System) == FileAttributes.System; } }

    /// <summary>Gets the MIME type of the file based on its extension.</summary>
    public string MimeType { get { return GetMimeType(); } }

    /// <summary>Gets the SHA256 hash of the file.</summary>
    public string Sha256 { get { return ComputeHash(SHA256.Create()); } }

    /// <summary>Gets the MD5 hash of the file.</summary>
    public string Md5 { get { return ComputeHash(MD5.Create()); } }

    /// <summary>Gets the SHA1 hash of the file.</summary>
    public string Sha1 { get { return ComputeHash(SHA1.Create()); } }

    /// <summary>Gets the SHA384 hash of the file.</summary>
    public string Sha384 { get { return ComputeHash(SHA384.Create()); } }

    /// <summary>Gets the SHA512 hash of the file.</summary>
    public string Sha512 { get { return ComputeHash(SHA512.Create()); } }

    /// <summary>
    /// Computes a hash of the file content using the specified algorithm.
    /// </summary>
    private string ComputeHash(HashAlgorithm algorithm)
    {
        using (var stream = FileInfo.OpenRead())
        {
            byte[] hash = algorithm.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    /// <summary>
    /// Gets the MIME type string based on file extension.
    /// </summary>
    private string GetMimeType()
    {
        string extension = Extension.ToLowerInvariant();
        switch (extension)
        {
            case ".txt": return "text/plain";
            case ".jpg":
            case ".jpeg": return "image/jpeg";
            case ".png": return "image/png";
            case ".gif": return "image/gif";
            case ".pdf": return "application/pdf";
            case ".doc":
            case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            case ".xls":
            case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            case ".exe": return "application/vnd.microsoft.portable-executable";
            case ".zip": return "application/zip";
            default: return "application/octet-stream";
        }
    }

    /// <summary>
    /// Compares two files byte by byte to determine if they are identical.
    /// </summary>
    public static bool FilesAreEqual(string path1, string path2, int bufferSize = 4096)
    {
        var file1 = new FileInfo(path1);
        var file2 = new FileInfo(path2);

        if (!file1.Exists || !file2.Exists)
            throw new FileNotFoundException("At least one file not found!");

        if (file1.Length != file2.Length)
            return false;

        using (var fs1 = file1.OpenRead())
        using (var fs2 = file2.OpenRead())
        {
            var buffer1 = new byte[bufferSize];
            var buffer2 = new byte[bufferSize];

            int bytesRead1, bytesRead2;
            do
            {
                bytesRead1 = fs1.Read(buffer1, 0, bufferSize);
                bytesRead2 = fs2.Read(buffer2, 0, bufferSize);

                if (bytesRead1 != bytesRead2)
                    return false;

                for (int i = 0; i < bytesRead1; i++)
                {
                    if (buffer1[i] != buffer2[i])
                        return false;
                }
            } while (bytesRead1 > 0);
        }

        return true;
    }

    /// <summary>
    /// Gets all unique file extensions from a specified folder.
    /// </summary>
    public static string[] GetAllFileExtensions(string folderPath, bool checkSubfolders)
    {
        HashSet<string> fileEndings = new HashSet<string>();
        string[] filePaths = Directory.GetFiles(folderPath, "*", checkSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (string filePath in filePaths)
        {
            fileEndings.Add(Path.GetExtension(filePath));
        }
        return new List<string>(fileEndings).ToArray();
    }

    public static bool IsValidPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        char[] invalidChars = Path.GetInvalidPathChars();
        return path.IndexOfAny(invalidChars) < 0;
    }

    /// <summary>
    /// Gets full file paths from a specified folder, optionally filtered by file extensions.
    /// </summary>
    /// <param name="folderPath">The folder to search in.</param>
    /// <param name="checkSubfolders">Whether to include subfolders in the search.</param>
    /// <param name="filter">Optional array of file extensions to include (e.g. ".txt", ".jpg"). Use null to include all files.</param>
    /// <returns>Array of full paths to matching files.</returns>
    public static string[] GetAllFilePaths(string folderPath, bool checkSubfolders, string[] filter = null)
    {
        List<string> filePathsResult = new List<string>();

        string[] filePaths = Directory.GetFiles(
            folderPath,
            "*",
            checkSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
        );

        foreach (string filePath in filePaths)
        {
            string extension = Path.GetExtension(filePath);

            if (filter != null && filter.Length > 0)
            {
                foreach (string ext in filter)
                {
                    if (string.IsNullOrWhiteSpace(ext) || extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        filePathsResult.Add(filePath);
                        break;
                    }
                }
            }
            else
            {
                filePathsResult.Add(filePath);
            }
        }

        return filePathsResult.ToArray();
    }

    /// <summary>
    /// Get the file name of a given path string.
    /// </summary>
    /// <param name="path">Path to file</param>
    /// <returns>Returns the filename of a given file path.</returns>
    public static string GetFileName(string path)
    {
        FileInfo newInfo = new FileInfo(path);
        return newInfo.Name;
    }

    /// <summary>
    /// Gets all file names from a specified folder, optionally filtered by file extensions.
    /// </summary>
    /// <param name="folderPath">The folder to search in.</param>
    /// <param name="checkSubfolders">Whether to include subfolders in the search.</param>
    /// <param name="filter">Optional array of file extensions to include (e.g. \".txt\", \".jpg\"). Use null to include all files.</param>
    /// <returns>Array of matching file names.</returns>
    public static string[] GetAllFileNames(string folderPath, bool checkSubfolders, string[] filter = null)
    {
        List<string> fileNames = new List<string>();

        string[] filePaths = Directory.GetFiles(
            folderPath,
            "*",
            checkSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
        );

        foreach (string filePath in filePaths)
        {
            string extension = Path.GetExtension(filePath);

            if (filter != null && filter.Length > 0)
            {
                foreach (string ext in filter)
                {
                    if (string.IsNullOrWhiteSpace(ext) || extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        fileNames.Add(Path.GetFileName(filePath));
                        break;
                    }
                }
            }
            else
            {
                fileNames.Add(Path.GetFileName(filePath));
            }
        }

        return fileNames.ToArray();
    }

    /// <summary>
    /// Renames a file to a new name, preserving its extension.
    /// </summary>
    public static void RenameFile(string filePath, string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName) && !string.IsNullOrWhiteSpace(filePath))
        {
            newName = newName.Trim();
            FileInfoExt fileInfo = new FileInfoExt(filePath);
            string extension = fileInfo.Extension;
            string directory = fileInfo.DirectoryName;
            string newFilePath = Path.Combine(directory, newName + extension);

            try
            {
                if (File.Exists(newFilePath))
                    throw new IOException("File '" + newFilePath + "' already exists.");

                File.Move(filePath, newFilePath);
            }
            catch (IOException ex)
            {
                throw new IOException("Failed renaming file old: " + filePath + ", new: " + newFilePath + ": " + ex.Message, ex);
            }
        }
    }

    /// <summary>
    /// Creates a directory if it does not already exist.
    /// </summary>
    public static void EnsureDirectoryExists(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        catch (Exception ex)
        {
            throw new IOException("Failed creating directory: " + path + " -> " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    public static void DeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            throw new IOException("Failed deleting file: " + path + " -> " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Copies a file to the specified target folder. If overwrite is false, a numeric suffix will be appended to avoid overwriting existing files.
    /// </summary>
    public static void CopyFile(string sourcePath, ref string targetPath, bool overwrite, int suffix = 0)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Source file missing", sourcePath);
        string targetDirectory = Path.GetDirectoryName(targetPath);
        if (!IsValidPath(targetDirectory))
            throw new ArgumentException("Target folder path is invalid", nameof(targetPath));
        EnsureDirectoryExists(targetDirectory);

        string filename = Path.GetFileName(sourcePath);

        bool areFilesEqual = false;

        if (!overwrite)
        {
            while (File.Exists(targetPath))
            {
                areFilesEqual = FilesAreEqual(sourcePath, targetPath);
                suffix++;
                string nameWithoutExt = Path.GetFileNameWithoutExtension(filename);
                string extension = Path.GetExtension(filename);
                if (areFilesEqual && !nameWithoutExt.EndsWith("_equal"))
                {
                    nameWithoutExt += "_equal";
                }
                targetPath = Path.Combine(targetDirectory, nameWithoutExt + "_" + suffix + extension);

                if (suffix > 1000)
                    throw new IOException("Failed to find a free suffix for file " + filename + " after 1000 attempts");
            }
        }

        try
        {
            File.Copy(sourcePath, targetPath, overwrite);
        }
        catch (IOException ex)
        {
            throw new IOException("Failed copying file: " + sourcePath + " -> " + targetPath + ": " + ex.Message, ex);
        }
    }
}