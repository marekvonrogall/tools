document.getElementById("uploadButton").addEventListener("click", async () => {
  const files = document.getElementById("fileInput").files;
  const status = document.getElementById("status");

  if (files.length === 0) {
    status.textContent = "Please select at least one file.";
    status.style.color = "red";
    return;
  }

  status.textContent = "Uploading...";
  status.style.color = "black";

  for (const file of files) {
    const formData = new FormData();
    formData.append("file", file);

    try {
      const response = await fetch("/music/upload", {
        method: "POST",
        body: formData,
      });

      if (response.ok) {
        const result = await response.json();
        status.textContent = `Uploaded: ${result.file}`;
        status.style.color = "green";
      } else {
        const errorText = await response.text();
        status.textContent = `Error: ${errorText}`;
        status.style.color = "red";
      }
    } catch (err) {
      console.error(err);
      status.textContent = "Upload failed.";
      status.style.color = "red";
    }
  }
});
