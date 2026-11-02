# Thought Trash Can

A calm 3D flower field with a writing table, three note pads and a trash can. People type or speak
a thought that is bothering them, crumple the sheet, and throw it in the can. An AI agent answers
with one or two gentle sentences, and can start a breathing guide (the table lantern pulses) when
the thought reads anxious. Nothing anyone writes is stored.

## What's inside

| Part | Tech |
|---|---|
| `ThoughtTrashCan.Api/wwwroot/index.html` | Three.js (WebGL) scene: instanced flower field with a wind shader, 360° orbit camera, canvas-textured note pads, crumple-and-throw animation, Web Speech / Azure Speech voice input |
| `ThoughtTrashCan.Api/ReleaseAgent.cs` | C# agent on **Azure OpenAI** (`Azure.AI.OpenAI` 2.x): JSON output, one function tool (`start_breathing`), capped 3-round tool loop, safety rule for crisis language |
| `ThoughtTrashCan.Api/SpeechTokenService.cs` | Swaps the **Azure AI Speech** key for a 10-minute browser token |
| `ThoughtTrashCan.Api/Program.cs` | ASP.NET Core 8 minimal API, static hosting, per-IP rate limiting |

API:
- `POST /api/release` `{ "text": "..." }` → `{ "kind", "reply", "urgent", "breatheCycles" }`
- `GET /api/speech/token` → `{ "token", "region" }`
- `GET /healthz`

If Azure OpenAI isn't configured, `/api/release` returns 404 and the page hides gentle replies.
If Azure Speech isn't configured, the mic falls back to the browser's built-in speech recognition.

## Run locally

```bash
cd ThoughtTrashCan.Api
dotnet user-secrets set "Azure:OpenAIEndpoint" "https://<your-resource>.openai.azure.com/"
dotnet user-secrets set "Azure:OpenAIKey" "<key>"            # or skip and use `az login`
dotnet user-secrets set "Azure:OpenAIDeployment" "gpt-4o-mini"
dotnet user-secrets set "Azure:SpeechKey" "<speech key>"     # optional
dotnet user-secrets set "Azure:SpeechRegion" "canadacentral" # optional
dotnet run
```

Open the printed `http://localhost:5xxx` address. Microphone access works on localhost and HTTPS.

## Deploy to Azure App Service

```bash
RG=thought-trash-can; LOC=canadacentral; APP=thought-trash-can-$RANDOM
az group create -n $RG -l $LOC
az appservice plan create -g $RG -n $APP-plan --sku B1 --is-linux
az webapp create -g $RG -p $APP-plan -n $APP --runtime "DOTNETCORE:8.0"
az webapp identity assign -g $RG -n $APP          # Managed Identity, no OpenAI key needed
# grant the identity "Cognitive Services OpenAI User" on your Azure OpenAI resource, then:
az webapp config appsettings set -g $RG -n $APP --settings \
  Azure__OpenAIEndpoint="https://<your-resource>.openai.azure.com/" \
  Azure__OpenAIDeployment="gpt-4o-mini" \
  Azure__SpeechKey="<speech key>" Azure__SpeechRegion="$LOC"
cd ThoughtTrashCan.Api && dotnet publish -c Release -o out && (cd out && zip -r ../app.zip .)
az webapp deploy -g $RG -n $APP --src-path app.zip --type zip
```

## Privacy and safety

- Thoughts are sent once for a reply and dropped. The API never logs request bodies; failures log only the status code.
- When the text suggests risk of self-harm or harming someone, the agent sets `urgent`, and the page keeps a
  card on screen pointing to 9-8-8 (call or text, Canada and US) until the person closes it.

## Ideas for next steps

- Swap Three.js for Babylon.js or a Unity WebGL build if you want physics-driven paper.
- A "keep it" tray for to-dos the agent recognises, so practical items aren't lost.
- Multi-user rooms (SignalR) where friends toss thoughts into the same can.
