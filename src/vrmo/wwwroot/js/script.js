const form = document.getElementById("login-form");

form.addEventListener("submit", async (e) => {
  e.preventDefault();

  const username = document.getElementById("username").value;
  const password = document.getElementById("password").value;

  try {
    const res = await fetch("/Auth/login", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ username, password }),
    });

    if (res.ok) {
      const data = await res.json();
      localStorage.setItem("vrmo-token", data.token);
      alert("Login erfolgreich!");
    } else {
      alert("Login fehlgeschlagen!");
    }
  } catch (err) {
    console.error("Login failed:", err);
  }
});
