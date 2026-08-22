/* =========================================================
   Velaris — animated simplex-noise WebGL gradient.
   Vanilla port of the React component (same GLSL, no deps).

   Usage:
     <div data-velaris
          data-bg="#03060a"
          data-colors="#0a3a3f,#146b5a,#1e9e7a,#04080c"
          data-speed="2.0"
          data-grain="0.3"></div>

   Falls back silently to the element's CSS background when
   WebGL is unavailable or the user prefers reduced motion.
   ========================================================= */
(function () {
    "use strict";

    var vertexShaderGLSL = [
        "attribute vec2 position;",
        "varying vec2 vUv;",
        "void main() {",
        "  vUv = position * 0.5 + 0.5;",
        "  gl_Position = vec4(position, 0.0, 1.0);",
        "}"
    ].join("\n");

    var fragmentShaderGLSL = [
        "precision highp float;",
        "varying vec2 vUv;",
        "",
        "uniform vec2  u_resolution;",
        "uniform float u_time;",
        "uniform float u_grain;",
        "uniform vec3  u_colors[4];",
        "uniform vec3  u_bg;",
        "",
        "vec3 permute(vec3 x) { return mod(((x*34.0)+1.0)*x, 289.0); }",
        "",
        "float snoise(vec2 v){",
        "  const vec4 C = vec4(0.211324865405187, 0.366025403784439,",
        "           -0.577350269189626, 0.024390243902439);",
        "  vec2 i  = floor(v + dot(v, C.yy) );",
        "  vec2 x0 = v -   i + dot(i, C.xx);",
        "  vec2 i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);",
        "  vec4 x12 = x0.xyxy + C.xxzz;",
        "  x12.xy -= i1;",
        "  i = mod(i, 289.0);",
        "  vec3 p = permute( permute( i.y + vec3(0.0, i1.y, 1.0 ))",
        "  + i.x + vec3(0.0, i1.x, 1.0 ));",
        "  vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy),",
        "    dot(x12.zw,x12.zw)), 0.0);",
        "  m = m*m ;",
        "  m = m*m ;",
        "  vec3 x = 2.0 * fract(p * C.www) - 1.0;",
        "  vec3 h = abs(x) - 0.5;",
        "  vec3 ox = floor(x + 0.5);",
        "  vec3 a0 = x - ox;",
        "  m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );",
        "  vec3 g;",
        "  g.x  = a0.x  * x0.x  + h.x  * x0.y;",
        "  g.yz = a0.yz * x12.xz + h.yz * x12.yw;",
        "  return 130.0 * dot(m, g);",
        "}",
        "",
        "void main() {",
        "  vec2 uv = vUv;",
        "  float ratio = u_resolution.x / u_resolution.y;",
        "  vec2 p = uv - 0.5;",
        "  p.x *= ratio;",
        "",
        "  float t = u_time * 0.1;",
        "",
        "  float n1 = snoise(p * 0.4 + vec2(t * 0.2, -t * 0.3));",
        "  float n2 = snoise(p * 0.55 + vec2(-t * 0.15, t * 0.25) + n1 * 0.25);",
        "  float n3 = snoise(p * 0.75 + vec2(t * 0.1, -t * 0.2) + n2 * 0.2);",
        "",
        "  vec3 col = u_bg;",
        "",
        "  float dist = length(p) * 1.5;",
        "  float vignette = 1.0 - smoothstep(0.3, 1.2, dist);",
        "",
        "  col = mix(col, u_colors[0], smoothstep(-0.2, 0.5, n1) * 0.85);",
        "  col = mix(col, u_colors[1], smoothstep(-0.1, 0.6, n2) * 0.7);",
        "  col = mix(col, u_colors[2], smoothstep(-0.3, 0.4, n3) * 0.6);",
        "  col = mix(col, u_colors[3], smoothstep(0.0, 0.7, n1 * n2) * 0.5);",
        "",
        "  float glow = smoothstep(0.8, 0.0, dist) * 0.3;",
        "  col += u_colors[1] * glow;",
        "",
        "  col = mix(col * 0.2, col, vignette);",
        "",
        "  float grain = fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453 + u_time);",
        "  col += (grain - 0.5) * u_grain * 0.1;",
        "",
        "  gl_FragColor = vec4(col, 1.0);",
        "}"
    ].join("\n");

    var DEFAULT_COLORS = ["#0a3a3f", "#146b5a", "#1e9e7a", "#04080c"];

    function hexToRgb(hex) {
        var h = String(hex).replace("#", "").trim();
        if (h.length === 3) {
            h = h[0] + h[0] + h[1] + h[1] + h[2] + h[2];
        }
        return [
            parseInt(h.slice(0, 2), 16) / 255,
            parseInt(h.slice(2, 4), 16) / 255,
            parseInt(h.slice(4, 6), 16) / 255
        ];
    }

    function createShader(gl, type, src) {
        var s = gl.createShader(type);
        gl.shaderSource(s, src);
        gl.compileShader(s);
        if (!gl.getShaderParameter(s, gl.COMPILE_STATUS)) {
            console.error("Velaris shader error:", gl.getShaderInfoLog(s));
            gl.deleteShader(s);
            return null;
        }
        return s;
    }

    function mount(container) {
        var bg = container.getAttribute("data-bg") || "#000000";

        var colorAttr = container.getAttribute("data-colors");
        var colors = colorAttr
            ? colorAttr.split(",").map(function (c) { return c.trim(); })
            : DEFAULT_COLORS;

        var speed = parseFloat(container.getAttribute("data-speed"));
        if (isNaN(speed)) speed = 2.0;

        var grain = parseFloat(container.getAttribute("data-grain"));
        if (isNaN(grain)) grain = 0.3;

        var canvas = document.createElement("canvas");
        canvas.className = "velaris-canvas";
        canvas.setAttribute("aria-hidden", "true");
        container.insertBefore(canvas, container.firstChild);

        var gl = canvas.getContext("webgl") ||
                 canvas.getContext("experimental-webgl");

        if (!gl) {
            // Static CSS gradient defined on .velaris stays visible.
            canvas.remove();
            return;
        }

        var vs = createShader(gl, gl.VERTEX_SHADER, vertexShaderGLSL);
        var fs = createShader(gl, gl.FRAGMENT_SHADER, fragmentShaderGLSL);
        if (!vs || !fs) {
            canvas.remove();
            return;
        }

        var program = gl.createProgram();
        gl.attachShader(program, vs);
        gl.attachShader(program, fs);
        gl.linkProgram(program);

        if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
            console.error("Velaris link error:", gl.getProgramInfoLog(program));
            canvas.remove();
            return;
        }

        gl.useProgram(program);

        var buffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
        gl.bufferData(
            gl.ARRAY_BUFFER,
            new Float32Array([-1, -1, 1, -1, -1, 1, 1, 1]),
            gl.STATIC_DRAW
        );

        var pos = gl.getAttribLocation(program, "position");
        gl.enableVertexAttribArray(pos);
        gl.vertexAttribPointer(pos, 2, gl.FLOAT, false, 0, 0);

        var locs = {
            res: gl.getUniformLocation(program, "u_resolution"),
            time: gl.getUniformLocation(program, "u_time"),
            grain: gl.getUniformLocation(program, "u_grain"),
            colors: gl.getUniformLocation(program, "u_colors"),
            bg: gl.getUniformLocation(program, "u_bg")
        };

        // Uniforms that never change — upload once.
        var bgRgb = hexToRgb(bg);
        gl.uniform3f(locs.bg, bgRgb[0], bgRgb[1], bgRgb[2]);
        gl.uniform1f(locs.grain, grain);

        var flat = new Float32Array(
            colors.slice(0, 4).reduce(function (acc, c) {
                return acc.concat(hexToRgb(c));
            }, [])
        );
        gl.uniform3fv(locs.colors, flat);

        function resize() {
            var dpr = Math.min(window.devicePixelRatio || 1, 2);
            canvas.width = Math.max(1, Math.floor(container.clientWidth * dpr));
            canvas.height = Math.max(1, Math.floor(container.clientHeight * dpr));
            gl.viewport(0, 0, canvas.width, canvas.height);
        }

        resize();

        if (typeof ResizeObserver !== "undefined") {
            new ResizeObserver(resize).observe(container);
        } else {
            window.addEventListener("resize", resize);
        }

        var reduced = window.matchMedia &&
            window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        function draw(time) {
            gl.uniform2f(locs.res, canvas.width, canvas.height);
            gl.uniform1f(locs.time, time * 0.001 * speed);
            gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);
        }

        if (reduced) {
            // Render a single frozen frame — no animation loop.
            draw(0);
            return;
        }

        var raf;
        function render(t) {
            draw(t);
            raf = requestAnimationFrame(render);
        }
        raf = requestAnimationFrame(render);

        // Stop burning GPU while the tab is hidden.
        document.addEventListener("visibilitychange", function () {
            if (document.hidden) {
                cancelAnimationFrame(raf);
            } else {
                raf = requestAnimationFrame(render);
            }
        });
    }

    function init() {
        var nodes = document.querySelectorAll("[data-velaris]");
        for (var i = 0; i < nodes.length; i++) {
            mount(nodes[i]);
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
