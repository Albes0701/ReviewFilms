# WebSocket Demo - Troubleshooting Guide

## Error: 405 Method Not Allowed

### What does it mean?
- The server rejected the request method (likely POST)
- Usually indicates the API isn't running or endpoint is misconfigured

### How to fix:

#### 1. **Verify API is Running**
```powershell
# From ReviewFilms directory:
dotnet run
# Output should show: Now listening on: http://localhost:5000
```

#### 2. **Check the API URL in Demo**
- Default: `http://localhost:5000`
- Common alternatives:
  - `http://127.0.0.1:5000`
  - `http://localhost:5001` (if HTTPS configured)

#### 3. **Verify CORS is Enabled**
Check [Program.cs](../Program.cs) has CORS configured:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

app.UseCors("AllowLocalhost");
```

#### 4. **Test Connection First**
- Click **"🧪 Test API"** button before connecting
- This will tell you if the API server is reachable
- Check logs for specific error messages

#### 5. **Verify Authentication Credentials**
- Username must exist in database
- Password must be correct
- Check `Microsoft.AspNetCore.Identity` is configured

### Step-by-Step Debugging:

1. **Open DevTools** in browser (F12)
2. **Go to Network tab**
3. **Click "Test API"** button
4. **Observe request:**
   - ✅ Status 200/204 = API is running properly
   - ❌ Status 405 = API routing issue or not running
   - ❌ Status 0 or timeout = Cannot reach server (firewall/port issue)

5. **Check Console tab** for detailed error messages

### Common Scenarios:

| Symptom | Likely Cause | Solution |
|---------|-------------|----------|
| 405 Method Not Allowed | API not running or endpoint mismatch | Run `dotnet run` in ReviewFilms folder |
| Connection timeout | API URL wrong or firewall blocking | Verify URL, check Windows Firewall |
| CORS error | CORS not enabled on backend | Add CORS middleware to Program.cs |
| Authentication fails | Wrong credentials | Verify username/password exist in DB |
| WebSocket connects but no messages | SignalR hub not configured | Ensure `MapHub<NotificationHub>()` in Program.cs |

### How to Run the Demo Correctly:

```bash
# Terminal 1: Start the API
cd D:\IT_K22\ReviewFilms
dotnet run
# Wait for: "Now listening on: http://localhost:5000"

# Terminal 2: Open HTML demo (optional - can also use VS Code Live Server)
# Option A: Use VS Code Live Server extension
# - Right-click docs/websocket-demo.html → Open with Live Server

# Option B: Use Python http server
python -m http.server 8000 --directory docs/

# Option C: Just open the file
# - Right-click websocket-demo.html → Open with browser
```

### URL Reference:
- **Demo URL:** `http://127.0.0.1:5500/websocket-demo.html` (if using Live Server on port 5500)
- **API URL:** `http://localhost:5000` (must be entered in demo settings)
- **Swagger/API Docs:** `http://localhost:5000/swagger/index.html`

### Still Having Issues?

1. Check that database is initialized:
   ```powershell
   dotnet ef database update
   # Or manually: mysql < migrations_script.sql
   ```

2. Verify test user exists:
   - Check ApplicationDbContext seed data
   - Or create user via `/api/auth/register` endpoint first

3. Check ReviewFilms logs:
   - Run API with: `dotnet run --verbose`
   - Look for middleware/SignalR diagnostic messages

4. Clear browser cache:
   - Hard refresh: `Ctrl+Shift+R` (or `Cmd+Shift+R` on Mac)
   - Clear cookies/storage in DevTools

