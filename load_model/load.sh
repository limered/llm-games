%%bash
# llamafile setup

# Step 1: Download a llamafile. The download may take several minutes.
wget -nv -nc https://huggingface.co/jartine/TinyLlama-1.1B-Chat-v1.0-GGUF/resolve/main/TinyLlama-1.1B-Chat-v1.0.Q5_K_M.llamafile

# Step 2: Make the llamafile executable. Note: if you're on Windows, just append '.exe' to the filename.
chmod +x TinyLlama-1.1B-Chat-v1.0.Q5_K_M.llamafile

# Step 3: Start llamafile server in background. All the server logs will be written to 'tinyllama.log'.
# Alternatively, you can just open a separate terminal outside this notebook and run: 
#   ./TinyLlama-1.1B-Chat-v1.0.Q5_K_M.llamafile --server --nobrowser --embedding
./TinyLlama-1.1B-Chat-v1.0.Q5_K_M.llamafile --server --nobrowser --embedding > tinyllama.log 2>&1 &
pid=$!
echo "${pid}" > .llamafile_pid  # write the process pid to a file so we can terminate the server later